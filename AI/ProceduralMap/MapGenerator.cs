using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cinemachine;
using Cinemachine.Utility;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace AI.ProceduralMap
{
    public enum Biome
    {
        None,
        Spawn,
        Plague,
        Death,
        War,
        Hunger,
        Lilith
    }

    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] MapSettings settings;
        
        public Vector2Int mapSize = new Vector2Int(1200, 800);
        public Vector2 cellSize = new Vector2(1, 1);
        [Range(0, 1)] public float fillRatio = 0.5f;
        public int n = 2;
        [Range(0, 1)] public float wallProxProb = 0.1f, staySameBioProb = 0.5f;
        public int minWallSize = 3, sameNeighbourToNone = 5;
        [SerializeField, Min(0)] int minSizeRegion;

        public float addNeighbourProb = 0.2f;
        public int biomeMinSize = 500;

        Cell[,] _cells;
        CellInfo[] _cellsInfo;
        public GameObject tilePrefab;
        public Transform map;

        public ComputeShader textureMaker;
        int _kernel;

        [Serializable]
        public class BiomeTextures
        {
            public Biome biome;
            public List<Texture2D> textures = new List<Texture2D>();
        }

        #region Placement Variables

        [Header("Object Placement Variables")]
        [SerializeField, Range(0, 5), Tooltip("Min distance between the center of each object. Lower values equal more item density")] float radius = 1;
        [SerializeField, Range(1, 50), Tooltip("Rejection Samples")] int k = 30;
        [SerializeField] BiomeObjects[] objects;
        [SerializeField] GameObject[] interactables;
        [SerializeField] GameObject lilithSpawner, plagueSpawner, warSpawner, hungerSpawner, deathSpawner;
        [SerializeField] GameObject player;
        [SerializeField] GameObject cinemachineCamera;
        [SerializeField, Range(0, 1), Tooltip("How likely to spawn an interactable")] float interactableProbability = 0.5f;
        
        Dictionary<Biome, IReadOnlyList<GameObject>> _objectsBiome;

        [Serializable]
        class BiomeObjects
        {
            [SerializeField] GameObject[] gameObject;
            [SerializeField] Biome biome;

            public Biome Biome => biome;

            public IReadOnlyList<GameObject> GameObject => gameObject;
        }    

        #endregion
        

        public class Region : List<Cell>
        {
            List<Vector2> _positions = new List<Vector2>();

            public Biome Biome { get; }
            public Bounds Bounds { get; set; }

            /// <summary>
            /// Gets the points limiting this region as a polygon. The outward most points
            /// </summary>
            public IReadOnlyList<Vector2> Positions => _positions;

            public Region(Biome biome)
            {
                Biome = biome;
            }

            public void SetPositions(List<Vector2> positions)
            {
                _positions = positions;
            }

            public void MergeWith(Region region)
            {
                AddRange(region);
                _positions.AddRange(region.Positions);
                // TODO: BOUNDS?
            }
        }
        
        List<Region> _regions;

        struct Edge : IEquatable<Edge>
        {
            public Edge(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
            }

            public Vector2 Start { get; set; }

            public Vector2 End { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj is Edge edge ? edge : default);
            }

            public bool Equals(Edge other)
            {
                return Start.Equals(other.Start) && End.Equals(other.End) || Start.Equals(other.End) && End.Equals(other.Start);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Start.GetHashCode() + End.GetHashCode();
                }
            }
        }

        public List<BiomeTextures> textBiome = new List<BiomeTextures>();
        Dictionary<Biome, List<Texture2D>> _texturesBiome = new Dictionary<Biome, List<Texture2D>>();

        void Awake()
        {
            LoadSettings();
            
            CreateRandomObjectsDictionary();
            
            _cells = new Cell[mapSize.x, mapSize.y];
            _cellsInfo = new CellInfo[mapSize.x * mapSize.y];

            _kernel = textureMaker.FindKernel("CSMain");

            _texturesBiome = new Dictionary<Biome, List<Texture2D>>();

            foreach (BiomeTextures biomeTextures in textBiome)
            {
                _texturesBiome[biomeTextures.biome] = biomeTextures.textures;
            }
            
            FillMap();

            for (int i = 0; i < n; i++)
            {
                ExecuteCells(); 
            }

            _regions = GetAllRegions();

            foreach (Cell cell in _regions.Where(region => region.Count < minSizeRegion).SelectMany(region => region))
            {
                cell.Biome = MostNeighbours(cell, out int _);
            }

            _regions = GetAllRegions(); // TODO: Optimize this
            
            ConnectRegions();

            foreach (Region region in _regions)
            {
                SetEdges(region);
            }
            
            SetColliders();
            
            SetTextures();
            GenerateSprites();

            PlaceObjects();
        }

        void ConnectRegions()
        {
            foreach (Biome biome in Enum.GetValues(typeof(Biome)))
            {
                if (biome == Biome.None)
                    continue;

                List<Region> regions = _regions.Where(r => r.Biome == biome).ToList();

                Region mainRegion = regions[0];
                int max = 0;
                int index = 0;
                for (int i = 0; i < regions.Count; i++)
                {
                    Region region = regions[i];
                    if (region.Count <= max)
                        continue;

                    index = i;
                    max = region.Count;
                    mainRegion = region;
                }
                
                regions.RemoveAt(index);

                float minDist = float.MaxValue;
                Cell mainCell = mainRegion[0];
                Cell connectCell = null;
                foreach (Region region in regions)
                {
                    foreach (Cell cell in region)
                    {
                        foreach (Cell cellMainRegion in mainRegion)
                        {
                            float dist = (cell.Index - cellMainRegion.Index).sqrMagnitude;
                            if (!(dist < minDist)) 
                                continue;
                            
                            minDist = dist;
                            connectCell = cell;
                            mainCell = cellMainRegion;
                        }
                    }
                    
                    mainRegion.MergeWith(region);
                    _regions.Remove(region);

                    if (connectCell == null)
                    {
                        print("Connection not found");
                        continue;
                    }
                    
                    Debug.DrawLine(mainCell.WorldPosition, connectCell.WorldPosition, Color.red, float.PositiveInfinity);
                    
                    foreach (Cell connectPoint in GetLine(mainCell.Index, connectCell.Index))
                    {
                        AddToMainRegion(mainRegion, connectPoint);

                        foreach (Cell neighbour in GetNeighbours(connectPoint)) 
                            AddToMainRegion(mainRegion, neighbour);
                    }
                }
            }
            
            List<Cell> GetLine(Vector2Int from, Vector2Int to) 
            {
                List<Cell> line = new List<Cell> ();

                int x = from.x;
                int y = from.y;

                int dx = to.x - from.x;
                int dy = to.y - from.y;

                bool inverted = false;
                int step = Math.Sign(dx);
                int gradientStep = Math.Sign(dy);

                int longest = Mathf.Abs(dx);
                int shortest = Mathf.Abs(dy);

                if (longest < shortest) 
                {
                    inverted = true;
                    longest = Mathf.Abs(dy);
                    shortest = Mathf.Abs(dx);

                    step = Math.Sign (dy);
                    gradientStep = Math.Sign (dx);
                }

                int gradientAccumulation = (int) (longest * 0.5f);
                
                for (int i =0; i < longest; i ++) 
                {
                    line.Add(_cells[x, y]);

                    if (inverted)
                        y += step;
                    else
                        x += step;

                    gradientAccumulation += shortest;
                    
                    if (gradientAccumulation < longest) 
                        continue;

                    if (inverted)
                        x += gradientStep;
                    else
                        y += gradientStep;
                    
                    gradientAccumulation -= longest;
                }

                return line;
            }

            void AddToMainRegion(Region main, Cell cell)
            {
                if (cell.Biome != Biome.None) 
                    return;
                
                cell.Biome = main.Biome;
                main.Add(cell);
            }
        }

        void LoadSettings()
        {
            Random.InitState(settings.Seed);
        }

        void PlaceObjects()
        {
            //Vector2[] points = PoissonDiscSampler.GeneratePoints(radius, mapSize, k);
            foreach (Region region in _regions.Where(r => r.Biome != Biome.None))
            {
                Vector2[] points = PoissonDiscSampler.GeneratePoints(radius, region, k);

                GameObject regionObject = new GameObject(region.Biome.ToString());
                GameObject interactablesObject = new GameObject("Interactables");
                interactablesObject.transform.parent = regionObject.transform;
                GameObject otherObject = new GameObject("Other")
                {
                    transform =
                    {
                        parent = regionObject.transform
                    }
                };

                if (points.Length == 0)
                {
                    print($"No points found in {region.Biome}");
                    continue;
                }
                
                // First point is to spawn the boss spawner
                Vector2 firstPoint = points[0];
                
                switch (region.Biome)
                {
                    case Biome.Spawn:
                        GameObject go = Instantiate(player, firstPoint, quaternion.identity);
                        GameObject cameraObject = Instantiate(cinemachineCamera);
                        CinemachineVirtualCamera vc = cameraObject.GetComponent<CinemachineVirtualCamera>();
                        vc.Follow = vc.LookAt = go.transform;
                        break;
                    case Biome.Plague:
                        Instantiate(plagueSpawner, firstPoint, Quaternion.identity, regionObject.transform);
                        break;
                    case Biome.Death:
                        Instantiate(deathSpawner, firstPoint, Quaternion.identity, regionObject.transform);
                        break;
                    case Biome.War:
                        Instantiate(warSpawner, firstPoint, Quaternion.identity, regionObject.transform);
                        break;
                    case Biome.Hunger:
                        Instantiate(hungerSpawner, firstPoint, Quaternion.identity, regionObject.transform);
                        break;
                    case Biome.Lilith:
                        Instantiate(lilithSpawner, firstPoint, Quaternion.identity, regionObject.transform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                for (int i = 1; i < points.Length; i++)
                {
                    Vector2 point = points[i];

                    if (Random.value <= interactableProbability)
                        PlaceRandomInteractable(point, interactablesObject.transform);
                    else
                        PlaceRandomObject(region, point, otherObject.transform);
                }
            }
            
            void PlaceRandomObject(Region region, Vector2 position, Transform parent)
            {
                IReadOnlyList<GameObject> possibleObjects = _objectsBiome[region.Biome];
                GameObject randomItem = possibleObjects[Random.Range(0, possibleObjects.Count)];
                Instantiate(randomItem, position, Quaternion.identity, parent);
            }

            void PlaceRandomInteractable(Vector2 position, Transform parent)
            {
                Instantiate(interactables[Random.Range(0, interactables.Length)], position, quaternion.identity, parent);
            }
        }

        void SetTextures()
        {
            foreach (Cell cell in _cells)
            {
                // List<Texture2D> availableTextures = _texturesBiome[cell.Biome];
                // cell.texture = availableTextures[Random.Range(0, availableTextures.Count)];
                int textIndex = GetTextureWithIndex(cell.Biome, out Texture2D texture);
                cell.Texture = texture;
                try
                {
                    _cellsInfo[cell.Index.x + cell.Index.y * mapSize.x] = new CellInfo((int) cell.Biome, textIndex);
                }
                catch (Exception e)
                {
                    print(e);
                    Debug.Log($"{cell.Index.x},{cell.Index.y}: Index: {cell.Index.x * cell.Index.y * mapSize.x} out of: {_cellsInfo.Length}");
                    throw;
                }
            }

            int GetTextureWithIndex(Biome biome, out Texture2D texture)
            {
                int index = 0;
                
                // for (int i = 0; i < (int)biome; i++)
                // {
                //     index += _texturesBiome[(Biome) i].Count;
                // }

                
                List<Texture2D> availableTextures = _texturesBiome[biome];
                int random = Random.Range(0, availableTextures.Count);

                texture = availableTextures[random];
 
                foreach (Texture2D t in _texturesBiome.Values.SelectMany(t => t))
                {
                    if (t == texture)
                    {
                        return index;
                    }
                    index++;
                }
                
                return -1; // ERROR
            }
        }

        void GenerateSprites()
        {
            // foreach (Transform child in map.transform)
            // {
            //     DestroyImmediate(child.gameObject);
            // }
            /*List<Task> tasks = new List<Task>(mapSize.x * mapSize.y);

            for (int y = 0; y < _cells.GetLength(1) - 1; y++)
            {
                for (int x = 0; x < _cells.GetLength(0) - 1; x++)
                {
                    Tile tile = Instantiate(tilePrefab, new Vector3(x, y, 0) * cellSize, quaternion.identity, map).GetComponent<Tile>();
                    
                    int x1 = x;
                    int y1 = y;

                    Cell topLeft = _cells[x1, y1 + 1];
                    Cell topRight = _cells[x1 + 1, y1 + 1];
                    Cell botLeft = _cells[x1, y1];
                    Cell botRight = _cells[x1 + 1, y1];
                    
                    tasks.Add(
                        Task.Run(() => GenerateTexture(topLeft, topRight, botLeft, botRight))
                            .ContinueWith(t =>
                            {
                                tile.CreateTile(t.Result);
                            }, TaskScheduler.FromCurrentSynchronizationContext()));
                }
            }*/

            // Sprites
            // textureMaker.SetTexture(_kernel, "topLeft", topLeft.texture);
            // textureMaker.SetTexture(_kernel, "topRight", topRight.texture);
            // textureMaker.SetTexture(_kernel, "botLeft", botLeft.texture);
            // textureMaker.SetTexture(_kernel, "botRight", botRight.texture);

            // Set corners
            // float tL = (float) topLeft.Biome;
            // float tR = (float) topRight.Biome;
            // float bL = (float) botLeft.Biome;
            // float bR = (float) botRight.Biome;
            //textureMaker.SetFloats("corners", tL, tR, bL, bR);

            // Set the size
            Vector2 size = 50 * (mapSize - Vector2Int.one); 
            Vector2 map_size = mapSize;
            textureMaker.SetVector("size", map_size);

            // Colors buffer
            int pixels = (int) (size.x * size.y);
            Color[] spriteInfo = new Color[pixels];
            ComputeBuffer resultBuffer = new ComputeBuffer(pixels, 16);
            resultBuffer.SetData(spriteInfo);
            textureMaker.SetBuffer(_kernel, "Result", resultBuffer);
            
            // Cell info
            ComputeBuffer cellsBuffer = new ComputeBuffer(
                _cellsInfo.Length,  8);
            cellsBuffer.SetData(_cellsInfo);
            textureMaker.SetBuffer(_kernel, "cells", cellsBuffer);
            
            // Set the textures
            int textCount = _texturesBiome.Values.Sum(list => list.Count);
            int pixelSize = 50 * textCount;
            Texture2D tempText = _texturesBiome[Biome.Death][0];
            Texture2DArray textures =
                new Texture2DArray(tempText.width, tempText.height, textCount, tempText.format, tempText.mipmapCount > 1)
                {
                    anisoLevel = tempText.anisoLevel,
                    filterMode = tempText.filterMode,
                    wrapMode = tempText.wrapMode
                };

            int index = 0; 
            foreach (Texture2D t in _texturesBiome.Values.SelectMany(texturesBiome => texturesBiome))
            {
                for (int m = 0; m < t.mipmapCount; m++)
                {
                    Graphics.CopyTexture(t, 0, m, textures, index, m);
                }

                index++;
            }
            
            textures.Apply();
            
            textureMaker.SetTexture(_kernel, "textures", textures);

            textureMaker.Dispatch(_kernel, (int)size.x / 10, (int)size.y / 10, 1);

            //Color[] colors = new Color[pixels];
            resultBuffer.GetData(spriteInfo);

            Texture2D texture = new Texture2D((int) size.x, (int) size.y);

            texture.SetPixels(spriteInfo);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size.x, size.y), Vector2.one * 0.5f, 50);

            // spriteRenderer.sprite = sprite;
            map.GetComponent<SpriteRenderer>().sprite = sprite;

            resultBuffer.Dispose();
            cellsBuffer.Dispose();
        }

        void FillMap()
        {
            List<Biome> biomes = new List<Biome>()
            {
                Biome.Death,
                Biome.Hunger,
                Biome.Lilith,
                Biome.Plague,
                Biome.War
            };
            List<Vector2Int> positions = new List<Vector2Int>()
            {
                mapSize / 2,
                new Vector2Int(mapSize.x / 2, mapSize.y / 5),
                new Vector2Int(mapSize.x / 5, mapSize.y / 2),
                new Vector2Int(mapSize.x * 4 / 5, mapSize.y / 2),
                new Vector2Int(mapSize.x / 5, mapSize.y * 2 / 3),
                new Vector2Int(mapSize.x * 4 / 5, mapSize.y * 2 / 3)
            };
            Queue<Biome> biomesLeft = new Queue<Biome>(biomes.OrderBy(b => Random.value));

            Vector2Int quadrantSize = mapSize / 3;
            int maxBiomeSize = (mapSize.x - minWallSize) * (mapSize.y - minWallSize) / 10;
            float minP = 0.05f;

            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    _cells[x, y] = new Cell(Biome.None, index, LocalToWorldPosition(index));
                }
            }

            Vector2Int spawnStart = mapSize / 2 + GetRandom();
            Vector2Int biome1Start =
                new Vector2Int(mapSize.x / 2 - mapSize.x / 5, mapSize.y - mapSize.y / 5) + GetRandom(); // Top Left
            Vector2Int biome2Start =
                new Vector2Int(mapSize.x / 2 + mapSize.x / 5, mapSize.y - mapSize.y / 5) + GetRandom(); // Top Right
            Vector2Int biome3Start =
                new Vector2Int(mapSize.x / 2 - mapSize.x / 5, mapSize.y / 2) + GetRandom(); // Middle Left
            Vector2Int biome4Start =
                new Vector2Int(mapSize.x / 2 + mapSize.x / 5, mapSize.y / 2) + GetRandom(); // Middle Right
            Vector2Int biome5Start = new Vector2Int(mapSize.x / 2, mapSize.y / 5) + GetRandom(); // Bot


            Cell spawn = _cells[spawnStart.x, spawnStart.y];
            spawn.Biome = Biome.Spawn;

            Cell biome1 = _cells[biome1Start.x, biome1Start.y];
            biome1.Biome = biomesLeft.Dequeue();

            Cell biome2 = _cells[biome2Start.x, biome2Start.y];
            biome2.Biome = biomesLeft.Dequeue();

            Cell biome3 = _cells[biome3Start.x, biome3Start.y];
            biome3.Biome = biomesLeft.Dequeue();

            Cell biome4 = _cells[biome4Start.x, biome4Start.y];
            biome4.Biome = biomesLeft.Dequeue();

            Cell biome5 = _cells[biome5Start.x, biome5Start.y];
            biome5.Biome = biomesLeft.Dequeue();

            Cell[] startCells = new[]
            {
                spawn, biome1, biome2, biome3, biome4, biome5,
            };

            Fill(startCells);


            // Vector2Int spawnLocation = mapSize / 2 + GetRandom();
            // Fill(Biome.Spawn, _cells[spawnLocation.x, spawnLocation.y]);
            // int index = 0;
            //
            // while (biomesLeft.Count > 0)
            // {
            //     Vector2Int pos = positions[index++] + GetRandom(); 
            //     Fill(biomesLeft.Dequeue(), _cells[pos.x, pos.y]);
            // }

            void Fill( /*Biome biome,*/ Cell[] cells)
            {
                Queue<Cell> toVisit = new Queue<Cell>(cells);
                //List<Cell> visited = new List<Cell>();
                bool[] visited = new bool[mapSize.x * mapSize.y]; // Optimized
                //toVisit.Enqueue(cell);

                int counter = 0;

                while (toVisit.Count > 0 /*&& cells < maxBiomeSize*/)
                {
                    Cell current = toVisit.Dequeue();

                    float prob = Mathf.Lerp(addNeighbourProb, 1, 1 / (counter / biomeMinSize * 6 + 1f));

                    foreach (Cell neighbour in GetNeighbours(current)
                        .Where(neighbour => neighbour.Biome == Biome.None
                                            && !toVisit.Contains(neighbour)
                                            && !visited[
                                                neighbour.Index.x +
                                                neighbour.Index.y * mapSize.x] /*!visited.Contains(neighbour)*/
                                            && Random.value < prob))
                    {
                        neighbour.Biome = current.Biome;

                        toVisit.Enqueue(neighbour);
                    }

                    // if (current.Biome != Biome.None)
                    //     print($"Replaced: {current.Biome} for {biome}");
                    // current.Biome = biome;

                    visited[current.Index.x + current.Index.y * mapSize.x] = true;
                    counter++;
                }
            }

            Vector2Int GetRandom() => new Vector2Int(Random.Range(-5, 6), Random.Range(-5, 6));

            // Loop through quadrants
            /*for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Biome nextBiome = Biome.None;

                    if (i == 1 && j == 1)
                    {
                        // Spawn is always in the center
                        FillQuadrant(j, i, Biome.Spawn);
                        continue;
                    }

                    if (biomesLeft.Count > 0)
                    {
                        float probability = Mathf.Lerp(0, 1, Mathf.Max(j + i * 3 / (float) biomes.Count, minP));
                        if (Random.value < probability)
                            nextBiome = biomesLeft.Dequeue();
                    }

                    FillQuadrant(j, i, nextBiome);
                }
            }

            void FillQuadrant(int quadrantX, int quadrantY, Biome biome)
            {
                for (int y = 0; y < quadrantSize.y; y++)
                {
                    for (int x = 0; x < quadrantSize.x; x++)
                    {
                        int xPos = x + quadrantX * quadrantSize.x;
                        int yPos = y + quadrantY * quadrantSize.y;
                        _cells[xPos, yPos] = Random.value < fillRatio
                            ? new Cell(Biome.None, new Vector2Int(xPos, yPos))
                            : new Cell(biome, new Vector2Int(xPos, yPos));
                    }
                }
            }*/
        }

        void ExecuteCells()
        {
            Cell[,] nextGen = new Cell[mapSize.x, mapSize.y];
            int amount = 0;

            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (x < minWallSize || y < minWallSize || x >= mapSize.x - minWallSize ||
                        y >= mapSize.y - minWallSize)
                    {
                        Vector2Int index = new Vector2Int(x, y);
                        nextGen[x, y] = new Cell(Biome.None, index, LocalToWorldPosition(index));
                        continue;
                    }

                    Cell cell = _cells[x, y];

                    int neighboursSameBiomes = NumberNeighbours(cell.Biome, cell);
                    int neighboursNoBiomes = NumberNeighbours(Biome.None, cell);

                    Biome mostNeighbours = MostNeighbours(cell, out amount);

                    Vector2Int pos = new Vector2Int(x, y);

                    float xWall = (float) x / mapSize.x;
                    float yWall = (float) y / mapSize.y;
                    float probability = Mathf.Lerp(0, wallProxProb,
                        (Mathf.Max(xWall, 1 / xWall) + Mathf.Max(yWall, 1 / yWall)) * 0.5f);

                    if (Random.value < probability)
                    {
                        nextGen[x, y] = new Cell(Biome.None, pos, LocalToWorldPosition(pos));
                    }
                    else if (neighboursSameBiomes == sameNeighbourToNone)
                    {
                        nextGen[x, y] = new Cell(Biome.None, pos, LocalToWorldPosition(pos));
                    }
                    else if (amount == 8)
                        nextGen[x, y] = new Cell(mostNeighbours, pos, LocalToWorldPosition(pos));
                    else
                    {
                        nextGen[x, y] = new Cell(Random.value < staySameBioProb ? cell.Biome : mostNeighbours, pos, LocalToWorldPosition(pos));
                    }

                    // if (cell.Biome == Biome.None && neighboursNoBiomes < 5)
                    // {
                    //     nextGen[x, y] = new Cell(mostNeighbours, pos);
                    // }
                    // else
                    // {
                    //     nextGen[x,y] = new Cell()
                    // }

                    /*if (neigboursSameBiome == 0)
                        nextGen[x, y] = new Cell(MostNeighbours(cell), pos);
                    // else if (neigboursSameBiome > 7)
                    // {
                    //     nextGen[x, y] = new Cell(Biome.None, pos);
                    // }
                    else if (cell.Biome == Biome.None && neigboursNoBiome > 5)
                        nextGen[x, y] = new Cell(MostNeighbours(cell), pos);
                    else if (neigboursNoBiome > 5)
                        nextGen[x, y] = new Cell(Biome.None, pos);
                    else
                        nextGen[x, y] = new Cell(cell.Biome, pos);*/
                }
            }

            _cells = nextGen;

            
        }

        List<Region> GetAllRegions()
        {
            List<Region> regions = new List<Region>();
            bool[,] flags = new bool[mapSize.x, mapSize.y];
            
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (!flags[x, y])
                        regions.Add(GetRegion(_cells[x, y], flags));
                }
            }

            return regions;
        }

        Region GetRegion(Cell cell, bool[,] flags)
        {
            Region regionCells = new Region(cell.Biome);
            //bool[,] flags = new bool[mapSize.x, mapSize.y];
            float minX = mapSize.x, maxX = 0, minY = mapSize.y, maxY = 0;
            Queue<Cell> queue = new Queue<Cell>();
            Enqueue(cell);

            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                regionCells.Add(current);
                
                // Check min and max
                Vector2 position = LocalToWorldPosition(current);
                if (position.x > maxX)
                    maxX = position.x;
                if (position.x < minX)
                    minX = position.x;
                if (position.y > maxY)
                    maxY = position.y;
                if (position.y < minY)
                    minY = position.y;
                
                foreach (Cell neighbour in GetNeighbours(current).Where(neighbour => neighbour.Biome == current.Biome && !flags[neighbour.Index.x, neighbour.Index.y]))
                {
                    Enqueue(neighbour);
                }   
            }

            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
            Vector3 center = new Vector3(minX + size.x / 2, minY + size.y / 2);
            regionCells.Bounds = new Bounds(center, size);

            List<Vector2> edgePoints = regionCells.Where(c => GetNeighbours(c).Any(n => n.Biome != c.Biome)).Select(c => c.WorldPosition).ToList();
            regionCells.SetPositions(edgePoints);

            return regionCells;

            void Enqueue(Cell addCell)
            {
                flags[addCell.Index.x, addCell.Index.y] = true;
                queue.Enqueue(addCell);
            }
        }
        
        List<Cell> GetNeighbours(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();

            if (cell.Index.x + 1 < mapSize.x)
                neighbours.Add(_cells[cell.Index.x + 1, cell.Index.y]);
            if (cell.Index.x - 1 > 0)
                neighbours.Add(_cells[cell.Index.x - 1, cell.Index.y]);
            if (cell.Index.y + 1 < mapSize.y)
                neighbours.Add(_cells[cell.Index.x, cell.Index.y + 1]);
            if (cell.Index.y - 1 > 0)
                neighbours.Add(_cells[cell.Index.x, cell.Index.y - 1]);

            return neighbours;
        }

        int NumberNeighbours(Biome biome, Cell cell)
        {
            int neighbours = 0;

            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int xPos = cell.Index.x + x;
                    int yPos = cell.Index.y + y;

                    if (xPos < 0 || xPos >= mapSize.x || yPos < 0 || yPos >= mapSize.y)
                        continue;

                    if (_cells[xPos, yPos].Biome == biome)
                        neighbours++;
                }
            }

            return neighbours;
        }

        Biome MostNeighbours(Cell cell, out int amount)
        {
            Dictionary<Biome, int> neighbours = new Dictionary<Biome, int>();

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int xPos = cell.Index.x + x;
                    int yPos = cell.Index.y + y;

                    if (xPos < 0 || xPos >= mapSize.x || yPos < 0 || yPos >= mapSize.y)
                        continue;

                    Biome biome = _cells[xPos, yPos].Biome;

                    // if (biome == Biome.None)
                    //     continue;

                    if (neighbours.TryGetValue(biome, out int _))
                    {
                        neighbours[_cells[xPos, yPos].Biome]++;
                    }
                    else
                    {
                        neighbours.Add(biome, 1);
                    }
                }
            }

            // if (neighbours.Values.Count == 0)
            // {
            //     amount = 0;
            //     return Biome.None;
            // }

            amount = neighbours.Values.Max();

            int i = amount;
            return neighbours.Keys.FirstOrDefault(biome => neighbours[biome] == i);
        }

        void SetColliders()
        {
            foreach (Region region in _regions.Where(r => r.Biome == Biome.None))
            {
                SetRegionCollider(region);
            }
        }

        void SetRegionCollider(Region region)
        {
            EdgeCollider2D edgeCollider2D = map.gameObject.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();

            List<Edge> edges = new List<Edge>();
            
            foreach (Cell cell in region.Where(c => GetNeighbours(c).Any(n => n.Biome != Biome.None)))
            {
                Cell top = _cells[cell.Index.x, cell.Index.y + 1];
                Cell bot = _cells[cell.Index.x, cell.Index.y - 1];
                Cell left = _cells[cell.Index.x - 1, cell.Index.y];
                Cell right = _cells[cell.Index.x + 1, cell.Index.y];

                if (top.Biome != Biome.None)
                {
                    Cell topLeft = _cells[top.Index.x - 1, top.Index.y];
                    Cell topRight = _cells[top.Index.x + 1, top.Index.y];
                    
                    Vector2 midTop = GetMidPoint(cell, top);
                    
                    if (left.Biome != Biome.None)
                    {
                        Vector2 midLeft = GetMidPoint(cell, left);
                        AddEdge(midLeft, midTop);
                    }
                    else if (topLeft.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(top, topLeft);
                        AddEdge(mid, midTop);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(left, _cells[top.Index.x - 1, top.Index.y]);
                        AddEdge(mid, midTop);
                    }
                    
                    if (right.Biome != Biome.None)
                    {
                        Vector2 midRight = GetMidPoint(cell, right);
                        AddEdge(midTop, midRight);
                    }
                    else if (topRight.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(top, topRight);
                        AddEdge(mid, midTop);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(right, _cells[top.Index.x + 1, top.Index.y]);
                        AddEdge(mid, midTop);
                    }
                }

                if (bot.Biome != Biome.None)
                {
                    Cell botLeft = _cells[bot.Index.x - 1, bot.Index.y];
                    Cell botRight = _cells[bot.Index.x + 1, bot.Index.y];
                        
                    Vector2 midBot = GetMidPoint(cell, bot);
                    
                    if (left.Biome != Biome.None)
                    {
                        Vector2 midLeft = GetMidPoint(cell, left);
                        AddEdge(midLeft, midBot);
                    }
                    else if (botLeft.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(bot, botLeft);
                        AddEdge(mid, midBot);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(left, botLeft);
                        AddEdge(mid, midBot);
                    }
                    
                    if (right.Biome != Biome.None)
                    {
                        Vector2 midRight = GetMidPoint(cell, right);
                        AddEdge(midBot, midRight);
                    }
                    else if (botRight.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(bot, botRight);
                        AddEdge(mid, midBot);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(right, botRight);
                        AddEdge(mid, midBot);
                    }
                }
                
                if (left.Biome != Biome.None)
                {
                    Cell topLeft = _cells[top.Index.x - 1, top.Index.y];
                    Cell botLeft = _cells[bot.Index.x - 1, bot.Index.y];
                        
                    Vector2 midLeft = GetMidPoint(cell, left);
                    
                    if (top.Biome != Biome.None)
                    {
                        Vector2 midPoint = GetMidPoint(cell, top);
                        AddEdge(midPoint, midLeft);
                    }
                    else if (topLeft.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(left, topLeft);
                        AddEdge(mid, midLeft);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(top, topLeft);
                        AddEdge(mid, midLeft);
                    }
                    
                    if (bot.Biome != Biome.None)
                    {
                        Vector2 midRight = GetMidPoint(cell, bot);
                        AddEdge(midLeft, midRight);
                    }
                    else if (botLeft.Biome == Biome.None)
                    {
                        Vector2 mid = GetMidPoint(left, botLeft);
                        AddEdge(mid, midLeft);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(bot, botLeft);
                        AddEdge(mid, midLeft);
                    }
                }
                
                if (right.Biome != Biome.None)
                {
                    Cell topRight = _cells[top.Index.x + 1, top.Index.y];
                    Cell botRight = _cells[bot.Index.x + 1, bot.Index.y];
                        
                    Vector2 midRight = GetMidPoint(cell, right);
                    
                    if (top.Biome != Biome.None)
                    {
                        Vector2 midPoint = GetMidPoint(cell, top);
                        AddEdge(midPoint, midRight);
                    }
                    else if (topRight.Biome == Biome.None)
                    {
                        Vector2 midPoint = GetMidPoint(right, topRight);
                        AddEdge(midPoint, midRight);
                    }
                    else
                    {
                        Vector2 midPoint = GetMidPoint(top, topRight);
                        AddEdge(midPoint, midRight);
                    }
                    
                    if (bot.Biome != Biome.None)
                    {
                        Vector2 midPoint = GetMidPoint(cell, bot);
                        AddEdge(midRight, midPoint);
                    }
                    else if (botRight.Biome == Biome.None)
                    {
                        Vector2 midPoint = GetMidPoint(right, botRight);
                        AddEdge(midPoint, midRight);
                    }
                    else
                    {
                        Vector2 midPoint = GetMidPoint(bot, botRight);
                        AddEdge(midPoint, midRight);
                    }
                }
            }

            Edge edge = edges[0];
            Vector2 end = edge.Start;
            points.Add(edge.Start);
            edges.RemoveAt(0);

            for (int i = edges.Count - 1; i >= 0; i--)
            {
                points.Add(edge.End);
                bool found = false;
                
                for (int j = 0; j < edges.Count && !found; j++)
                {
                    Edge next = edges[j];

                    if (edge.End.Equals(next.End))
                    {
                        edge = edges[j];
                        edges.Remove(edge);
                        found = true;
                        
                        // Swap end and start
                        (edge.Start, edge.End) = (edge.End, edge.Start);
                    }
                    else if (edge.End.Equals(next.Start))
                    {
                        edge = edges[j];
                        edges.Remove(edge);
                        found = true;
                    }
                }
            }

            //FindNext(0);
            
            points.Add(end);

            edgeCollider2D.points = points.ToArray();

            Vector2 GetMidPoint(Cell a, Cell b)
            {
                // if (a.Biome != Biome.None || b.Biome == Biome.None)
                //     print("WRONG BIOMES");
                
                Vector2 dir = b.Index - a.Index;

                Vector2 worldPos = LocalToWorldPosition(a) + dir * 0.5f;

                return worldPos;
            }

            void AddEdge(Vector2 start, Vector2 edgeEnd)
            {
                Edge newEdge = new Edge(start, edgeEnd);
                
                if (!edges.Contains(newEdge))
                    edges.Add(newEdge);
            }
        }

        void SetEdges(Region region)
        {
            List<Vector2> points = new List<Vector2>();
            List<Edge> edges = new List<Edge>();
            Biome regionBiome = region.Biome;
            
            foreach (Cell cell in region.Where(c => GetNeighbours(c).Any(n => n.Biome != regionBiome)))
            {
                Cell top = _cells[cell.Index.x, cell.Index.y + 1];
                Cell bot = _cells[cell.Index.x, cell.Index.y - 1];
                Cell left = _cells[cell.Index.x - 1, cell.Index.y];
                Cell right = _cells[cell.Index.x + 1, cell.Index.y];

                if (top.Biome != regionBiome)
                {
                    Cell topLeft = _cells[top.Index.x - 1, top.Index.y];
                    Cell topRight = _cells[top.Index.x + 1, top.Index.y];
                    
                    Vector2 midTop = GetMidPoint(cell, top);
                    
                    if (left.Biome != regionBiome)
                    {
                        Vector2 midLeft = GetMidPoint(cell, left);
                        AddEdge(midLeft, midTop);
                    }
                    else if (topLeft.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(top, topLeft);
                        AddEdge(mid, midTop);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(left, _cells[top.Index.x - 1, top.Index.y]);
                        AddEdge(mid, midTop);
                    }
                    
                    if (right.Biome != regionBiome)
                    {
                        Vector2 midRight = GetMidPoint(cell, right);
                        AddEdge(midTop, midRight);
                    }
                    else if (topRight.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(top, topRight);
                        AddEdge(mid, midTop);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(right, _cells[top.Index.x + 1, top.Index.y]);
                        AddEdge(mid, midTop);
                    }
                }

                if (bot.Biome != regionBiome)
                {
                    Cell botLeft = _cells[bot.Index.x - 1, bot.Index.y];
                    Cell botRight = _cells[bot.Index.x + 1, bot.Index.y];
                        
                    Vector2 midBot = GetMidPoint(cell, bot);
                    
                    if (left.Biome != regionBiome)
                    {
                        Vector2 midLeft = GetMidPoint(cell, left);
                        AddEdge(midLeft, midBot);
                    }
                    else if (botLeft.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(bot, botLeft);
                        AddEdge(mid, midBot);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(left, botLeft);
                        AddEdge(mid, midBot);
                    }
                    
                    if (right.Biome != regionBiome)
                    {
                        Vector2 midRight = GetMidPoint(cell, right);
                        AddEdge(midBot, midRight);
                    }
                    else if (botRight.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(bot, botRight);
                        AddEdge(mid, midBot);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(right, botRight);
                        AddEdge(mid, midBot);
                    }
                }
                
                if (left.Biome != regionBiome)
                {
                    Cell topLeft = _cells[top.Index.x - 1, top.Index.y];
                    Cell botLeft = _cells[bot.Index.x - 1, bot.Index.y];
                        
                    Vector2 midLeft = GetMidPoint(cell, left);
                    
                    if (top.Biome != regionBiome)
                    {
                        Vector2 midPoint = GetMidPoint(cell, top);
                        AddEdge(midPoint, midLeft);
                    }
                    else if (topLeft.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(left, topLeft);
                        AddEdge(mid, midLeft);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(top, topLeft);
                        AddEdge(mid, midLeft);
                    }
                    
                    if (bot.Biome != regionBiome)
                    {
                        Vector2 midRight = GetMidPoint(cell, bot);
                        AddEdge(midLeft, midRight);
                    }
                    else if (botLeft.Biome == regionBiome)
                    {
                        Vector2 mid = GetMidPoint(left, botLeft);
                        AddEdge(mid, midLeft);
                    }
                    else
                    {
                        Vector2 mid = GetMidPoint(bot, botLeft);
                        AddEdge(mid, midLeft);
                    }
                }
                
                if (right.Biome != regionBiome)
                {
                    Cell topRight = _cells[top.Index.x + 1, top.Index.y];
                    Cell botRight = _cells[bot.Index.x + 1, bot.Index.y];
                        
                    Vector2 midRight = GetMidPoint(cell, right);
                    
                    if (top.Biome != regionBiome)
                    {
                        Vector2 midPoint = GetMidPoint(cell, top);
                        AddEdge(midPoint, midRight);
                    }
                    else if (topRight.Biome == regionBiome)
                    {
                        Vector2 midPoint = GetMidPoint(right, topRight);
                        AddEdge(midPoint, midRight);
                    }
                    else
                    {
                        Vector2 midPoint = GetMidPoint(top, topRight);
                        AddEdge(midPoint, midRight);
                    }
                    
                    if (bot.Biome != regionBiome)
                    {
                        Vector2 midPoint = GetMidPoint(cell, bot);
                        AddEdge(midRight, midPoint);
                    }
                    else if (botRight.Biome == regionBiome)
                    {
                        Vector2 midPoint = GetMidPoint(right, botRight);
                        AddEdge(midPoint, midRight);
                    }
                    else
                    {
                        Vector2 midPoint = GetMidPoint(bot, botRight);
                        AddEdge(midPoint, midRight);
                    }
                }
                
                Edge edge = edges[0];
                Vector2 end = edge.Start;
                points.Add(edge.Start);
                edges.RemoveAt(0);

                for (int i = edges.Count - 1; i >= 0; i--)
                {
                    points.Add(edge.End);
                    bool found = false;
                
                    for (int j = 0; j < edges.Count && !found; j++)
                    {
                        Edge next = edges[j];

                        if (edge.End.Equals(next.End))
                        {
                            edge = edges[j];
                            edges.Remove(edge);
                            found = true;
                        
                            // Swap end and start
                            (edge.Start, edge.End) = (edge.End, edge.Start);
                        }
                        else if (edge.End.Equals(next.Start))
                        {
                            edge = edges[j];
                            edges.Remove(edge);
                            found = true;
                        }
                    }
                }
                
                region.SetPositions(points);
            }
            
            Vector2 GetMidPoint(Cell a, Cell b)
            {
                // if (a.Biome != Biome.None || b.Biome == Biome.None)
                //     print("WRONG BIOMES");
                
                Vector2 dir = b.Index - a.Index;

                Vector2 worldPos = LocalToWorldPosition(a) + dir * 0.5f;

                return worldPos;
            }

            void AddEdge(Vector2 start, Vector2 edgeEnd)
            {
                Edge newEdge = new Edge(start, edgeEnd);
                
                if (!edges.Contains(newEdge))
                    edges.Add(newEdge);
            }
        }

        Vector2 LocalToWorldPosition(Cell cell) => LocalToWorldPosition(cell.Index);

        Vector2 LocalToWorldPosition(Vector2 index) => index - (mapSize - Vector2.one) / 2;

        /*Biome GetBiome(Vector2 position)
        {
            // Get the 4 cells surrounding
            int minX = (int) (position.x / cellSize.x);
            int minY = (int) (position.y / cellSize.y);
            Cell topLeft = _cells[minX, minY + 1];
            Cell topRight = _cells[minX + 1, minY + 1];
            Cell botLeft = _cells[minX, minY];
            Cell botRight = _cells[minX + 1, minY];
            
            
        }*/

        void CreateRandomObjectsDictionary()
        {
            _objectsBiome = new Dictionary<Biome, IReadOnlyList<GameObject>>();
            foreach (BiomeObjects biomeObjects in objects)
            {
                try
                {
                    _objectsBiome.Add(biomeObjects.Biome, biomeObjects.GameObject);
                }
                catch (Exception e)
                {
                    print($"Couldn't add object to dictionary. ERROR: {e}");
                }
            }
        }

#if UNITY_EDITOR

        Dictionary<Biome, Color> _colors = new Dictionary<Biome, Color>()
        {
            {Biome.None, Color.black},
            {Biome.Death, Color.blue},
            {Biome.Hunger, Color.yellow},
            {Biome.Lilith, Color.magenta},
            {Biome.Plague, Color.green},
            {Biome.Spawn, Color.white},
            {Biome.War, Color.red}
        };

        List<Color> colors = new List<Color>();

        public float cubeSize = 0.5f;
        public bool showPoints = false;
        public bool showOutline = false;

        void OnDrawGizmos()
        {
            // for (int y = 0; y < _cells.GetLength(1); y++)
            // {
            //     for (int x = 0; x < _cells.GetLength(0); x++)
            //     {
            //         Gizmos.color = _colors[_cells[x, y].Biome];
            //         Gizmos.DrawCube(new Vector2(x * cellSize.x, y * cellSize.y), Vector3(cubeSize));
            //     }
            // }
            
            if (_cells == null)
                return;

            if (showPoints)
            {
                int i = 0;
                foreach (Region region in _regions)
                {
                    Gizmos.color = colors[i++];
                    foreach (Cell cell in region)
                    {
                        Gizmos.DrawSphere(LocalToWorldPosition(cell), 0.2f);
                    }
                }
            }

            if (showOutline)
            {
                foreach (Region region in _regions)
                {
                    Gizmos.color = _colors[region.Biome];
                    foreach (Vector2 position in region.Positions)
                    {
                        Gizmos.DrawSphere(position, 0.1f);
                    }
                }
            }
        }

        void OnValidate()
        {
            CreateRandomObjectsDictionary();

            colors = new List<Color>();

            for (int i = 0; i < 50; i++)
            {
                colors.Add(Random.ColorHSV());
            }
        }
        
        static Vector3 ToVector3(float a) => new Vector3(a, a, a);
#endif
    }
}