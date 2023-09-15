using System;
using System.Collections.Generic;
using System.Linq;
using AI.PathFinding;
using JetBrains.Annotations;
using UnityEngine;
using Grid = AI.PathFinding.Grid;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = "Pathfinder Database")]
    public class PathFinderDatabase : ScriptableObject
    {
        [NonSerialized] readonly Dictionary<Grid, PathFinder> _grids = new Dictionary<Grid, PathFinder>();
        
        public float MinX { get; private set; } 
        public float MinY { get; private set; } 
        public float MaxX { get; private set; }
        public float MaxY { get; private set; }
        
        public void AddGrid(Grid grid, PathFinder pathFinder)
        {
            if (_grids.TryGetValue(grid, out _))
                return;
            
            _grids.Add(grid, pathFinder);
            UpdateMinMax();
        }

        public bool TryFindPath(Vector3 start, Vector3 target, out Vector3[] path)
        {
            // Figure out the grid/pathfinder to use
            if (TryGetPathFinder(start, out PathFinder pathFinder))
                return pathFinder.TryFindPath(start, target, out path);

            path = null;
            return false;
        }

        bool TryGetPathFinder(Vector3 start, out PathFinder pathFinder)
        {
            foreach (Grid grid in _grids.Keys.Where(grid => grid.Bounds.Contains(start)))
            {
                pathFinder = _grids[grid];
                return true;
            }
            
            pathFinder = null;
            return false;
        }

        void UpdateMinMax()
        {
            MinX = _grids.Keys.Min(g => g.Bounds.xMin);
            MinY = _grids.Keys.Min(g => g.Bounds.yMin);
            MaxX = _grids.Keys.Max(g => g.Bounds.xMax);
            MaxY = _grids.Keys.Max(g => g.Bounds.yMax);
        }
    }
}
