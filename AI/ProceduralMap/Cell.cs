using UnityEngine;

namespace AI.ProceduralMap
{
    public class Cell 
    {
        public Biome Biome { get; set; } 
        public Vector2Int Index { get; }
        public Vector2 WorldPosition { get; }
        public Texture2D Texture { get; set; } 

        public Cell(Biome biome, Vector2Int index, Vector2 worldPosition)
        {
            Biome = biome;
            Index = index;
            WorldPosition = worldPosition;
        }
    }

    /// <summary>
    /// Info to send to gpu
    /// </summary>
    public struct CellInfo
    {
        public int Biome;
        public int TextureIndex;

        public CellInfo(int biome, int textureIndex)
        {
            Biome = biome;
            TextureIndex = textureIndex;
        }
    }
}
