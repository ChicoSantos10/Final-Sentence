using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Items;
using UnityEngine;

namespace Craft
{
    [Serializable]
    public class Ingredient 
    {
        [SerializeField] InventoryItem item;
        [SerializeField, Min(1)] int quantity = 1;

        public int Quantity => quantity;

        public InventoryItem Item => item;
    }
}
