using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace AI.PathFinding
{
        class Heap<T> where T : IHeapItem<T>
        {
            T[] items;
            int itemCount;

            /// <summary>
            /// The amount of items currently in this heap
            /// </summary>
            public int ItemCount { get => itemCount; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="size">The max size for the heap<param>
            public Heap(int size)
            {
                items = new T[size];
            }

            /// <summary>
            /// Adds an item to the heap
            /// </summary>
            /// <param name="item">the item to be added</param>
            public void Add(T item)
            {
                item.Index = itemCount;
                items[itemCount] = item;

                SortUp(item);

                itemCount++;
            }

            /// <summary>
            /// Removes the first element in the heap
            /// </summary>
            /// <returns>The first element</returns>
            public T RemoveFirst()
            {
                T item = items[0];

                itemCount--;

                items[0] = items[itemCount];
                items[0].Index = 0;

                SortDown(items[0]);

                return item;
            }

            /// <summary>
            /// Sorts an item down the heap
            /// </summary>
            /// <param name="item"></param>
            void SortDown(T item)
            {
                // Getting the indexes of the child item
                int childIndexLeft = item.Index * 2 + 1;
                int childIndexRight = item.Index * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < itemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < itemCount)
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                            swapIndex = childIndexRight;

                    if (item.CompareTo(items[swapIndex]) < 0)
                        Swap(item, items[swapIndex]);
                    else
                        return;
                }
                else
                    return;

                SortDown(item);
            }

            /// <summary>
            /// Sorts up to find the correct position in the heap
            /// </summary>
            /// <param name="item"></param>
            void SortUp(T item)
            {
                // Formula for the parent index in the heap
                int parentIndex = (item.Index - 1) / 2;

                T parentItem = items[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                    Swap(item, parentItem);
                else
                    return;

                SortUp(item);
            }

            /// <summary>
            /// Swaps the two items
            /// </summary>
            /// <param name="itemA"></param>
            /// <param name="itemB"></param>
            void Swap(T itemA, T itemB)
            {
                items[itemA.Index] = itemB;
                items[itemB.Index] = itemA;

                // Swap
                (itemA.Index, itemB.Index) = (itemB.Index, itemA.Index);
            }

            /// <summary>
            /// Checks if the item is in the heap
            /// </summary>
            /// <param name="item">The item to be checked</param>
            /// <returns>true if it's in the heap</returns>
            public bool Contains(T item)
            {
                return Equals(items[item.Index], item);
            }

            /// <summary>
            /// Updates the position of an item
            /// </summary>
            /// <param name="item"></param>
            public void UpdateItem(T item)
            {
                SortUp(item);
            }
        }

        public interface IHeapItem<T> : IComparable<T>
        {
            int Index { get; set; }
        }
    
}
