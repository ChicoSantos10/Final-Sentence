using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI.ProceduralMap
{
    public static class PoissonDiscSampler
    {
        /// <summary>
        /// Generates random points using poisson disc distribution
        /// </summary>
        /// <param name="radius">The min distance between each point</param>
        /// <param name="sampleRegion">The size of the region to sample the points for</param>
        /// <param name="activeList">The initial points</param>
        /// <param name="k">Number of samples before rejection</param>
        /// <returns></returns>
        public static Vector2[] GeneratePoints(float radius, Vector2 sampleRegion, int k = 30)
        {
            const float twoPi = 2 * Mathf.PI;
            float cellSize = radius / Mathf.Sqrt(2);

            Vector2Int gridSize = new Vector2Int(Mathf.CeilToInt(sampleRegion.x / cellSize), Mathf.CeilToInt(sampleRegion.y / cellSize));
            int[] grid = GetGrid(gridSize); // Stores the index of the point in each cell
            List<Vector2> points = new List<Vector2>();
            List<Vector2> activeList = new List<Vector2> {sampleRegion / 2};
            
            while (activeList.Count > 0)
            {
                int index = Random.Range(0, activeList.Count);
                Vector2 point = activeList[index];

                bool found = false;
                for (int i = 0; i < k && !found; i++)
                {
                    float angle = Random.value * twoPi;
                    float dist = Random.Range(radius, 2 * radius);
                    Vector2 candidate = point + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                    found = IsValid(candidate, sampleRegion, cellSize, radius, points, grid, gridSize);

                    if (!found) 
                        continue;
                    
                    points.Add(candidate);
                    activeList.Add(candidate);

                    grid[GetIndex(candidate, sampleRegion)] = points.Count - 1;
                }

                if (!found) 
                    activeList.RemoveAt(index);
            }

            return points.ToArray();
        }
        
        /// <summary>
        /// Generates points inside a region
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="sampleRegion"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Vector2[] GeneratePoints(float radius, MapGenerator.Region sampleRegion, int k = 30)
        {
            const float twoPi = 2 * Mathf.PI;
            float cellSize = radius / Mathf.Sqrt(2);

            Vector2 sampleRegionSize = sampleRegion.Bounds.extents * 2;
            //Vector2Int gridSize = new Vector2Int(Mathf.CeilToInt(sampleRegion.Bounds.extents.x * 2 / cellSize), Mathf.CeilToInt(sampleRegion.Bounds.extents.y * 2 / cellSize));
            Vector2Int gridSize = RoundUpVector(sampleRegionSize / cellSize);
            int[] grid = GetGrid(gridSize); // Stores the index of the point in each cell
            List<Vector2> points = new List<Vector2>();
            List<Vector2> activeList = new List<Vector2> {sampleRegion.Positions[Random.Range(0, sampleRegion.Positions.Count)]};
            
            while (activeList.Count > 0)
            {
                int index = Random.Range(0, activeList.Count);
                Vector2 point = activeList[index];

                bool found = false;
                for (int i = 0; i < k && !found; i++)
                {
                    float angle = Random.value * twoPi;
                    float dist = Random.Range(radius, 2 * radius);
                    Vector2 candidate = point + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                    found = IsValid(candidate, sampleRegion, cellSize, radius, points, grid, gridSize);

                    if (!found) 
                        continue;
                    
                    points.Add(candidate);
                    activeList.Add(candidate);

                    //int j = GetIndex(candidate, sampleRegion.Bounds.extents * 2);
                    Vector2 positionCentered = PositionCentered(sampleRegion, candidate);
                    int j = GetIndex(positionCentered, cellSize, gridSize);
                    try
                    {
                        grid[j] = points.Count - 1;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(j);
                    }
                }

                if (!found) 
                    activeList.RemoveAt(index);
            }

            return points.ToArray();
        }

        static int GetIndex(Vector2 positionCentered, float cellSize, Vector2Int gridSize)
        {
            int j = (int) (positionCentered.x / cellSize) + (int) (positionCentered.y / cellSize) * gridSize.x;
            return j;
        }

        static Vector2 PositionCentered(MapGenerator.Region sampleRegion, Vector2 candidate)
        {
            Vector2 positionCentered = candidate + (Vector2) sampleRegion.Bounds.extents - (Vector2) sampleRegion.Bounds.center;
            return positionCentered;
        }

        static bool IsValid(Vector2 point, Vector2 sampleRegion, float cellSize, float radius, IReadOnlyList<Vector2> points, IReadOnlyList<int> grid, Vector2Int gridSize)
        {
            // Out of bounds
            if (point.x < 0 || point.x >= sampleRegion.x || point.y < 0 || point.y >= sampleRegion.y)
                return false;
            
            int cellX = (int)(point.x / cellSize);
            int cellY = (int)(point.y / cellSize);
            int startX = Mathf.Max(0,cellX - 2);
            int endX = Mathf.Min(cellX + 2, gridSize.x);
            int startY = Mathf.Max(0,cellY - 2);
            int endY = Mathf.Min(cellY + 2, gridSize.y);

            for (int x = startX; x <= endX; x++) 
            {
                for (int y = startY; y <= endY; y++) 
                {
                    int i = grid[x + y * gridSize.x];
                    
                    // No point in the grid
                    if (i == -1) 
                        continue;
                    
                    float sqrDst = (point - points[i]).sqrMagnitude;
                    if (sqrDst < radius*radius) 
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        static bool IsValid(Vector2 point, MapGenerator.Region sampleRegion, float cellSize, float radius, IReadOnlyList<Vector2> points, IReadOnlyList<int> grid, Vector2Int gridSize)
        {
            // Out of bounds
            if (!IsPointInside(sampleRegion.Positions, point))
                return false;

            Vector2 positionCentered = PositionCentered(sampleRegion, point);
            int cellX = (int)(positionCentered.x / cellSize);
            int cellY = (int)(positionCentered.y / cellSize);
            int startX = Mathf.Max(0,cellX - 2);
            int endX = Mathf.Min(cellX + 2, gridSize.x - 1);
            int startY = Mathf.Max(0,cellY - 2);
            int endY = Mathf.Min(cellY + 2, gridSize.y - 1);

            for (int x = startX; x <= endX; x++) 
            {
                for (int y = startY; y <= endY; y++) 
                {
                    int i = grid[x + y * gridSize.x];
                    
                    // No point in the grid
                    if (i == -1) 
                        continue;
                    
                    float sqrDst = (point - points[i]).sqrMagnitude;
                    if (sqrDst < radius*radius) 
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        static bool IsPointInside(IReadOnlyList<Vector2> points, Vector2 point)
        {
            bool result = false;
            int j = points.Count - 1;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].y < point.y && points[j].y >= point.y || points[j].y < point.y && points[i].y >= point.y)
                {
                    if (points[i].x + (point.y - points[i].y) / (points[j].y - points[i].y) * (points[j].x - points[i].x) < point.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        
        static int GetIndex(Vector2 pos, Vector2 size) => (int) (pos.x / size.x + pos.y / size.y * size.x);

        static int[] GetGrid(Vector2Int size)
        {
            int[] grid = new int[size.x * size.y];

            for (int index = 0; index < grid.Length; index++)
            {
                grid[index] = -1;
            }

            return grid;
        }

        static Vector2Int RoundUpVector(Vector2 vector)
        {
            return new Vector2Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
        }
    }
}
