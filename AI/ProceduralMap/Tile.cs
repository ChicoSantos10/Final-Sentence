using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.ProceduralMap
{
    [Flags]
    internal enum ActiveCorners
    {
        None = 0,
        TopLeft = 1 << 1, 
        TopRight = 1 << 2, 
        BotLeft = 1 << 3, 
        BotRight = 1 << 4
    }
    
    public class Tile : MonoBehaviour
    {
        [SerializeField] ComputeShader shader;
        [SerializeField] Texture2D topLeftTexture, topRightTexture, botLeftTexture, botRightTexture;
        [SerializeField] ActiveCorners corners;
        [SerializeField] int size = 50;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] bool useComputeShader, useColors;
        [SerializeField] Color colorTopLeft, colorTopRight, colorBotRight, colorBotLeft;
        [SerializeField, Range(0, 0.5f)] float threshold;
        [SerializeField] float noiseOffset, noiseScale;

        Cell _topLeft, _botLeft, _topRight, _botRight;
        Vector2Int _position;

        public void CreateTile(/*Cell topLeft, Cell botLeft, Cell topRight, Cell botRight, Vector2Int position, int kernel*/Sprite sprite)
        {
            // _topLeft = topLeft;
            // _botLeft = botLeft;
            // _topRight = topRight;
            // _botRight = botRight;
            // _position = position;

            //BuildWithComputeShader(kernel);

            spriteRenderer.sprite = sprite;
        }

        // void OnValidate()
        // {
        //     if (useComputeShader)
        //         BuildWithComputeShader();
        //     else
        //         Build();
        // }

        void BuildWithComputeShader(int kernel)
        {
            
            
            
        }

        void Build()
        {
            Texture2D texture = new Texture2D(size, size);
            
            float tL = Convert.ToSingle(corners.HasFlag(ActiveCorners.TopLeft));
            float tR = Convert.ToSingle(corners.HasFlag(ActiveCorners.TopRight));
            float bL = Convert.ToSingle(corners.HasFlag(ActiveCorners.BotLeft));
            float bR = Convert.ToSingle(corners.HasFlag(ActiveCorners.BotRight));

            if (useColors)
                UseColors(texture);
            else
                UseTextures(texture);
            
            //SetColliders();

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0 , size, size), Vector2.one * 0.5f, 50);

            spriteRenderer.sprite = sprite;
        }
        
        void UseTextures(Texture2D texture)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float xRatio = (size - 1 - i) / (float)(size - 1);
                    float yRatio = (size - 1 - j) / (float)(size - 1);

                    float blWeight = xRatio * yRatio;
                    float brWeight = (1 - xRatio) * yRatio;
                    float tlWeight = xRatio * (1 - yRatio);
                    float trWeight = (1 - xRatio) * (1 - yRatio);

                    //Dictionary<Texture2D, float> weights = new Dictionary<Texture2D, float> {{topLeftTexture, tlWeight}};
                    Dictionary<Biome, float> weights = new Dictionary<Biome, float> {{_topLeft.Biome, tlWeight}};

                    // if (weights.TryGetValue(colorTopRight, out float _))
                    // {
                    //     weights[colorTopRight] += trWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorTopRight, trWeight);
                    // }
                    AddToDictionary(weights, _topRight.Biome, trWeight);
                    
                    // if (weights.TryGetValue(colorBotLeft, out float _))
                    // {
                    //     weights[colorBotLeft] += blWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorBotLeft, blWeight);
                    // }
                    AddToDictionary(weights, _botLeft.Biome, blWeight);

                    // if (weights.TryGetValue(colorBotRight, out float _))
                    // {
                    //     weights[colorBotRight] += brWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorBotRight, brWeight);
                    // }
                    AddToDictionary(weights, _botRight.Biome, brWeight);

                    Color finalColor;
                    Biome finalBiome = Biome.None;
                    Texture2D finalTexture = Texture2D.redTexture;
                    float max = 0;
                    foreach (KeyValuePair<Biome, float> pair in weights.Where(pair => pair.Value > max))
                    {
                        max = pair.Value;
                        //finalTexture = pair.Key;
                        finalBiome = pair.Key;
                    }

                    tlWeight *= Convert.ToSingle(finalBiome == _topLeft.Biome); // Math to check if tl weight should be considered 
                    trWeight *= Convert.ToSingle(finalBiome == _topRight.Biome);
                    blWeight *= Convert.ToSingle(finalBiome == _botLeft.Biome); 
                    brWeight *= Convert.ToSingle(finalBiome == _botRight.Biome);

                    finalColor = tlWeight * _topLeft.Texture.GetPixel(i, j) +
                                 trWeight * _topRight.Texture.GetPixel(i, j) +
                                 blWeight * _botLeft.Texture.GetPixel(i, j) +
                                 brWeight * _botRight.Texture.GetPixel(i, j);
                    
                    
                    // Uncomment this
                    /*float maxByDistance = Mathf.Max(Mathf.Max(tlWeight, trWeight), Mathf.Max(blWeight, brWeight));
                    
                    if (maxByDistance < threshold)
                    {
                        float[] cornerWeights = {trWeight, tlWeight, blWeight, brWeight};

                        float secondMax = 0;
                        foreach (float weight in cornerWeights)
                        {
                            if (weight < max && weight > secondMax)
                                secondMax = weight;
                        }

                        Texture2D firstTexture;
                        if (Math.Abs(maxByDistance - tlWeight) < Mathf.Epsilon)
                            firstTexture = topLeftTexture;
                        else if (Math.Abs(maxByDistance - trWeight) < Mathf.Epsilon)
                            firstTexture = topRightTexture;
                        else if (Math.Abs(maxByDistance - blWeight) < Mathf.Epsilon)
                            firstTexture = botLeftTexture;
                        else
                            firstTexture = botRightTexture;
                        
                        Texture2D secondTexture;
                        if (Math.Abs(secondMax - tlWeight) < Mathf.Epsilon)
                            secondTexture = topLeftTexture;
                        else if (Math.Abs(secondMax - trWeight) < Mathf.Epsilon)
                            secondTexture = topRightTexture;
                        else if (Math.Abs(secondMax - blWeight) < Mathf.Epsilon)
                            secondTexture = botLeftTexture;
                        else
                            secondTexture = botRightTexture;

                        finalColor = finalTexture.GetPixel(i, j);
                        Color secondColor = secondTexture.GetPixel(i, j);
                        
                         float x = noiseOffset + i / (float) size * noiseScale;
                         float y = noiseOffset + j / (float) size * noiseScale;
                         float t = Mathf.PerlinNoise(x, y);

                        finalColor = Color.Lerp(finalColor, secondColor, Mathf.RoundToInt(t));
                    }
                    else
                    {
                        finalColor = finalTexture.GetPixel(i, j);
                    }*/ // To Here

                    /*Color topLeftColor = topLeftTexture.GetPixel(i, j);
                    Color topRightColor = topRightTexture.GetPixel(i, j);
                    Color botLeftColor = botLeftTexture.GetPixel(i, j);
                    Color botRightColor = botRightTexture.GetPixel(i, j);

                    Color second = Color.clear;
                    float t = 0;
                    if (maxByDistance < threshold)
                    {
                        float x = noiseOffset + i / (float) size * noiseScale;
                        float y = noiseOffset + j / (float) size * noiseScale;
                        t = Mathf.PerlinNoise(x, y);
                        float maxW = 0;
                        float secMax = 0;
                        foreach (float weight in new []{tlWeight, trWeight, blWeight, brWeight})
                        {
                            if (!(weight > maxW)) 
                                continue;
                            
                            secMax = maxW;
                            maxW = weight;
                        }
                        if ()
                    }
                    
                    tlWeight = Mathf.Floor(tlWeight + (1 - maxByDistance)); 
                    trWeight = Mathf.Floor(trWeight + (1 - maxByDistance)); 
                    blWeight = Mathf.Floor(blWeight + (1 - maxByDistance)); 
                    brWeight = Mathf.Floor(brWeight + (1 - maxByDistance)); 
                        
                    finalColor = topLeftTexture.GetPixel(i, j) * tlWeight +
                                 topRightTexture.GetPixel(i, j) * trWeight +
                                 botLeftTexture.GetPixel(i, j) * blWeight +
                                 botRightTexture.GetPixel(i, j) * brWeight;

                    finalColor = Color.Lerp(finalColor, second, Mathf.RoundToInt(t));*/
                    texture.SetPixel(i, j, finalColor);
                }      
            }
        }

        void UseColors(Texture2D texture)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float xRatio = (size - i) / (float)size;
                    float yRatio = (size - j) / (float) size;

                    float blWeight = xRatio * yRatio;
                    float brWeight = (1 - xRatio) * yRatio;
                    float tlWeight = xRatio * (1 - yRatio);
                    float trWeight = (1 - xRatio) * (1 - yRatio);

                    Dictionary<Color, float> weights = new Dictionary<Color, float> {{colorTopLeft, tlWeight}};

                    // if (weights.TryGetValue(colorTopRight, out float _))
                    // {
                    //     weights[colorTopRight] += trWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorTopRight, trWeight);
                    // }
                    AddToDictionary(weights, colorTopRight, trWeight);
                    
                    // if (weights.TryGetValue(colorBotLeft, out float _))
                    // {
                    //     weights[colorBotLeft] += blWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorBotLeft, blWeight);
                    // }
                    AddToDictionary(weights, colorBotLeft, blWeight);

                    // if (weights.TryGetValue(colorBotRight, out float _))
                    // {
                    //     weights[colorBotRight] += brWeight;
                    // }
                    // else
                    // {
                    //     weights.Add(colorBotRight, brWeight);
                    // }
                    AddToDictionary(weights, colorBotRight, brWeight);

                    Color finalColor = Color.clear;
                    float max = 0;
                    foreach (KeyValuePair<Color, float> pair in weights.Where(pair => pair.Value > max))
                    {
                        max = pair.Value;
                        finalColor = pair.Key;
                    }
                    
                    float maxByDistance = Mathf.Max(Mathf.Max(tlWeight, trWeight), Mathf.Max(blWeight, brWeight));
                    
                    if (maxByDistance < threshold)
                    {
                        float[] cornerWeights = new[] {trWeight, tlWeight, blWeight, brWeight};

                        float secondMax = 0;
                        for (int k = 0; k < cornerWeights.Length; k++)
                        {
                            float weight = cornerWeights[k];
                            if (weight < max && weight > secondMax)
                                secondMax = weight;
                        }

                        Color firstColor;
                        if (Math.Abs(maxByDistance - tlWeight) < Mathf.Epsilon)
                            firstColor = colorTopLeft;
                        else if (Math.Abs(maxByDistance - trWeight) < Mathf.Epsilon)
                            firstColor = colorTopRight;
                        else if (Math.Abs(maxByDistance - blWeight) < Mathf.Epsilon)
                            firstColor = colorBotLeft;
                        else
                            firstColor = colorBotRight;
                        
                        Color secondColor;
                        if (Math.Abs(secondMax - tlWeight) < Mathf.Epsilon)
                            secondColor = colorTopLeft;
                        else if (Math.Abs(secondMax - trWeight) < Mathf.Epsilon)
                            secondColor = colorTopRight;
                        else if (Math.Abs(secondMax - blWeight) < Mathf.Epsilon)
                            secondColor = colorBotLeft;
                        else
                            secondColor = colorBotRight;
                        
                        float x = noiseOffset + i / (float) size * noiseScale;
                        float y = noiseOffset + j / (float) size * noiseScale;
                        float t = Mathf.PerlinNoise(x, y);

                        finalColor = Color.Lerp(finalColor, secondColor, Mathf.RoundToInt(t));
                    }

                    // Color finalColor = Color.Lerp(botColor, topColor, Mathf.RoundToInt(t));
                    //
                    texture.SetPixel(i, j, finalColor);
                }      
            }
        }

        void AddToDictionary<T>(IDictionary<T, float> weights, T key, float value)
        {
            if (weights.TryGetValue(key, out _))
                weights[key] += value;
            else
                weights.Add(key, value);
        }

        /*void SetColliders()
        {
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();

            List<Vector2> points = new List<Vector2>();
            //Vector2 transform.

            // Top 
            if (!corners.HasFlag(ActiveCorners.TopLeft))
            {
                points.Add(new Vector2(-0.5f, 0.5f));
                points.Add(Vector2.up * 0.5f);

                // if (!corners.HasFlag(ActiveCorners.TopRight))
                //     points.Add(new Vector2(0.5f, 0.5f));
                // else
                //     points.Add(Vector2.up * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.TopRight))
            {
                points.Add(Vector2.up * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.TopRight))
            {
                points.Add(new Vector2(0.5f, 0.5f));
                points.Add(Vector2.right * 0.5f);
                
                // if (!corners.HasFlag(ActiveCorners.BotRight))
                //     points.Add(new Vector2(0.5f, -0.5f));
                // else
                //     points.Add(Vector2.right * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.BotRight))
            {
                points.Add(Vector2.right * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.BotRight))
            {
                points.Add(new Vector2(0.5f, -0.5f));
                points.Add(Vector2.down * 0.5f);

                // if (!corners.HasFlag(ActiveCorners.BotLeft))
                //     points.Add(new Vector2(-0.5f, -0.5f));
                // else
                //     points.Add(Vector2.down * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.BotLeft))
            {
                points.Add(Vector2.down * 0.5f);
            }

            if (!corners.HasFlag(ActiveCorners.BotLeft))
            {
                points.Add(new Vector2(-0.5f, -0.5f));
                points.Add(Vector2.left * 0.5f);

                if (corners.HasFlag(ActiveCorners.TopLeft))
                    points.Add(Vector2.left * 0.5f);
                // else
                //     points.Add(new Vector2(-0.5f, 0.5f));
            }

            if (!corners.HasFlag(ActiveCorners.TopLeft))
            {
                points.Add(Vector2.left * 0.5f);
            }

            if (points.Count > 0)
            {
                edgeCollider.enabled = true;
                points.Add(points[0]);
                edgeCollider.points = points.ToArray();
            }
            else
                edgeCollider.enabled = false;
        }*/
    }
}
