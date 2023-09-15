using System;
using UnityEngine;

namespace Scriptable_Objects.Items.Recipes
{
    //[CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(InventoryItemRecipe))]
    public abstract class InventoryItemRecipe : Recipe
    {
        //[SerializeField] InventoryItem item;

        [SerializeField] Inventory inventory;
        [SerializeField, Min(1)] int addQuantity = 1;

        //public override Item Item => item;

        public override bool Craft()
        {
            inventory.AddItem(Item as InventoryItem, addQuantity);

            return false;
        }

        void OnValidate()
        {
            if (!(Item is InventoryItem))
                Debug.LogError($"{Item} needs to be of type {typeof(InventoryItem)}");
        }
    }
}
