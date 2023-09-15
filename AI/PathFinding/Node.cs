using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.PathFinding {

    public class Node : IHeapItem<Node>
    {
        /// <summary>
        /// Cost from starting node
        /// </summary>
        public float CostG { get; set; }

        /// <summary>
        /// Cost to target node
        /// </summary>
        public float CostH { get; set; }

        /// <summary>
        /// Is this node walkable
        /// </summary>
        public bool IsWalkable { get; set; }
        
        /// <summary>
        /// Position in the grid
        /// </summary>
        public Vector2Int GridIndex { get; }
        
        /// <summary>
        /// The position in world coordinates
        /// </summary>
        public Vector3 WorldPosition { get; }
        
        /// <summary>
        /// The parent of this node to be used for pathfinding purposes
        /// </summary>
        public Node Parent { get; set; }
      
        /// <summary>
        /// The estimated cost to reach destination
        /// </summary>
        public float EstimatedTotalCost => CostG + CostH;

        /// <summary>
        /// Index of this item in the heap
        /// </summary>
        public int Index { get; set; }

        public Node(int posX, int posY, Vector3 worldPosition, bool walkable)
        {
            IsWalkable = walkable;
            GridIndex = new Vector2Int(posX, posY);
            WorldPosition = worldPosition;
        }
        
        /// <summary>
        /// Compares the cost between 2 nodes
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Node other)
        {
            int comparableCost = other.EstimatedTotalCost.CompareTo(this.EstimatedTotalCost);

            if (comparableCost.Equals(0))
            {
                comparableCost = other.CostH.CompareTo(this.CostH);
            }

            return comparableCost;
        }
    }
}
