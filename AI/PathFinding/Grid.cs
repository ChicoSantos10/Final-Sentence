using System;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects;
using Scriptable_Objects.Items.Recipes;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AI.PathFinding
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] List<TileBase> nonWalkable;
        [SerializeField] LayerMask collidersNonWalkable;

        Node[,] _grid;
        int _gridSizeX, _gridSizeY;
        Tilemap _tilemap;

        /// <summary>
        /// The amount of nodes
        /// </summary>
        public int Size => _gridSizeX * _gridSizeY;

        public Rect Bounds => new Rect(_tilemap.CellToWorld(_tilemap.origin), new Vector2(_gridSizeX, _gridSizeY));

        void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            _tilemap.CompressBounds();
            
            Vector3Int size = _tilemap.size;
            _gridSizeX = size.x;
            _gridSizeY = size.y;

            CreateGrid();
        }

        void OnEnable()
        {
            BuildingRecipe.BuildingPlacedEvent += BuildingPlaced;
        }

        void OnDisable()
        {
            BuildingRecipe.BuildingPlacedEvent -= BuildingPlaced;
        }

        void BuildingPlaced(Vector3 position)
        {
            if (!TryGetNodeFromWorld(position, out Node node))
                return;
                
            node.IsWalkable = false;

            Queue<Node> checkNodes = new Queue<Node>();
            List<Node> checkedNodes = new List<Node>();
            checkNodes.Enqueue(node);

            while (checkNodes.Count > 0)
            {
                Node current = checkNodes.Dequeue();

                foreach (Node neighbour in GetNeighbours(current).Where(neighbour => !checkedNodes.Contains(neighbour)))
                {
                    if (HasCollision(neighbour.WorldPosition))
                    {
                        neighbour.IsWalkable = false;
                        checkNodes.Enqueue(neighbour);
                    }
                    
                    checkedNodes.Add(neighbour);
                }
            }
        }

        /// <summary>
        /// Create the grid
        /// </summary>
        void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];

            //create nodes
            for (int i = 0; i < _gridSizeY; i++)
            {
                for (int j = 0; j < _gridSizeX; j++)
                {   
                    // The origin is the bottom left corner
                    Vector3Int origin = _tilemap.origin;
                    Vector3Int currentPosition = new Vector3Int(j + origin.x,i + origin.y , 0);
                    Vector3 worldPosition = _tilemap.GetCellCenterWorld(currentPosition);
                    bool walkable;

                    if (_tilemap.HasTile(currentPosition) && !nonWalkable.Contains(_tilemap.GetTile(currentPosition)) && !HasCollision(worldPosition))
                        walkable = true;
                    else
                        walkable = false;

                    _grid[j, i] = new Node(j, i, worldPosition, walkable);
                }
            }
        }

        /// <summary>
        /// Get current node neighbours
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for(int y = -1; y <= 1; y++)
            {
                for(int x = -1; x <= 1; x++)
                {
                    //if x== 0 or y == 0, it means that's the current node, not the neighbours
                    if (x == 0 && y == 0)
                        continue;

                    int neighbourX = node.GridIndex.x + x;
                    int neighbourY = node.GridIndex.y + y;

                    if(neighbourX < _gridSizeX && neighbourY < _gridSizeY && neighbourX > 0 && neighbourY > 0)
                    {
                        //8 neighbours total
                        neighbours.Add(_grid[neighbourX, neighbourY]);
                    }
                }
            }

            return neighbours;
        }

        bool HasCollision(Vector3 worldPosition)
        {
            List<Collider2D> colliders = new List<Collider2D>();

            Vector2 positionA = worldPosition + new Vector3(-0.5f, 0.5f);
            Vector2 positionB = worldPosition + new Vector3(0.5f, -0.5f);

            ContactFilter2D filter = new ContactFilter2D
            {
                layerMask = collidersNonWalkable,
                useLayerMask = true,
            };

            int n = Physics2D.OverlapArea(positionA, positionB, filter, colliders);

            for (int i = 0; i < n; i++)
            {
                if (!colliders[i].isTrigger)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the node from world
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Node GetNodeFromWorld(Vector3Int position)
        {
            return _grid[position.x, position.y];
        }

        public bool TryGetNodeFromWorld(Vector2 position, out Node node)
        {
            Vector3Int localPosition = _tilemap.WorldToCell(position);

            if (_tilemap.GetTile(localPosition) == null)
            {
                node = null;
                return false;
            }

            localPosition -= _tilemap.origin; // Sets the tilemap on origin (bot left to be (0,0,0)
            //localPosition += new Vector3Int(_gridSizeX / 2, _gridSizeY / 2, 0); 

            if (localPosition.x > 0 && localPosition.x < _gridSizeX && localPosition.y > 0 &&
                localPosition.y < _gridSizeY)
            {
                node = GetNodeFromWorld(localPosition);
                return true;
            }

            node = null;
            return false;
        }

#if UNITY_EDITOR
        
        [Header("DEBUG")] public bool drawGizmos;
        
        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;
            
            if (_grid != null)
            {
                foreach (Node n in _grid) 
                {
                    Gizmos.color = n.IsWalkable ? new Color(0,0,1,0.5f) : new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawCube(n.WorldPosition, Vector3.one);
                }
            }

            if (pathTest != null)
            {
                Gizmos.color = Color.white;
                
                for (int i = 0; i < pathTest.Length - 1; i++)
                {
                    Handles.DrawAAPolyLine(pathTest[i], pathTest[i + 1]);
                }
            }
        }

        Vector3[] pathTest;

        PathFinder path;

#endif
    }
}
