using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scriptable_Objects;
using Unity.Profiling;

namespace AI.PathFinding
{
    [RequireComponent (typeof(Grid))]
    public class PathFinder : MonoBehaviour
    {
        // Profiling
        static readonly ProfilerMarker ProfilerMarker = new ProfilerMarker(ProfilerCategory.Ai, "Path Finder");
        
        [SerializeField] PathFinderDatabase database;
        Grid _grid;      
        
        void Awake()
        {
            _grid = GetComponent<Grid>();
        }

        void Start()
        {
            database.AddGrid(_grid, this);
        }

        /// <summary>
        /// find possible path
        /// </summary>
        public bool TryFindPath(Vector2 start, Vector2 target, out Vector3[] path)
        {
            using ProfilerMarker.AutoScope scope = ProfilerMarker.Auto();

            path = null;
            if (start == target)
                return false;
            
            Heap<Node> nodesOpen = new Heap<Node>(_grid.Size);
            HashSet<Node> nodesClosed = new HashSet<Node>();

            // Nodes arent in the grid
            if (!_grid.TryGetNodeFromWorld(start, out Node startNode) || !_grid.TryGetNodeFromWorld(target, out Node targetNode))
                return false;

            // We assume start node is walkable or if it isn't it prevents enemies from becoming stuck
            if (/*!startNode.IsWalkable ||*/ !targetNode.IsWalkable)
                return false;

            // Already at the destination
            if (startNode == targetNode)
            {
                path = new Vector3[]{target};
                return true;
            }
            
            nodesOpen.Add(startNode);

            while (nodesOpen.ItemCount > 0)
            {
                Node currentNode = nodesOpen.RemoveFirst();
                nodesClosed.Add(currentNode);
                
                if (currentNode == targetNode)
                {
                    path = RetracePath(startNode, targetNode, target);
                    return true;
                }                  

                foreach(Node neighbour in _grid.GetNeighbours(currentNode))
                {
                    // skip the non walkable nodes or those that are on the close set
                    if (!neighbour.IsWalkable || nodesClosed.Contains(neighbour))
                        continue;

                    // calculate the new cost, distance to the current
                    float newCost = currentNode.CostG + GetDistance(currentNode, neighbour);

                    // check neighbours cost 
                    if(newCost < neighbour.CostG || !nodesOpen.Contains(neighbour))
                    {
                        neighbour.CostG = newCost;
                        neighbour.CostH = GetDistance(neighbour, targetNode);

                        neighbour.Parent = currentNode;

                        if (!nodesOpen.Contains(neighbour))
                            nodesOpen.Add(neighbour);
                        else
                            nodesOpen.UpdateItem(neighbour);
                    }
                }
            }

            //if didnt find a valid path
            Debug.Log("No valid path");
            Debug.DrawLine(start, target, Color.black);
            Debug.Break();
            return false;
        }

        /// <summary>
        /// Distance between nodes
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        static float GetDistance(Node nodeA, Node nodeB)
        {
            float x = Mathf.Abs(nodeA.WorldPosition.x - nodeB.WorldPosition.x);
            float y = Mathf.Abs(nodeA.WorldPosition.y - nodeB.WorldPosition.y);
            float distance = Mathf.Sqrt(x * x + y * y);  

            return distance;
        }

        /// <summary>
        /// Retrace path running backwards 
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="targetNode"></param>
        /// <param name="targetPos"></param>
        Vector3[] RetracePath(Node startNode, Node targetNode, Vector3 targetPos) 
        {
            List<Vector3> path = new List<Vector3> {targetPos};
            Node currentNode = targetNode.Parent;
            
            while (currentNode != startNode)
            {
                path.Add(currentNode.WorldPosition);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path.ToArray();
        }
    }
}
