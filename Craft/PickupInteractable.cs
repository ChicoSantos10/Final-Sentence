using System;
using Scriptable_Objects;
using Scriptable_Objects.Items;
using UnityEngine;

namespace Craft
{
    [RequireComponent(typeof(SpriteRenderer))] 
    public class PickupInteractable : Interactable
    {
        [SerializeField] InventoryItem item;
        [SerializeField] Inventory inventory;
        [SerializeField] int removeAmount = 1;
        [SerializeField] int maxAmount = 50;

        int _currentAmount;

        void Start()
        {
            _currentAmount = maxAmount;
        }

        protected override void OnInteractActionCompleted()
        {
            int quantity = removeAmount > _currentAmount ? _currentAmount : removeAmount;
            
            inventory.AddItem(item, quantity);

            _currentAmount -= quantity;
            
            if (_currentAmount <= 0)
                Destroy(gameObject);
        }

        void OnValidate()
        {
            GetComponent<SpriteRenderer>().sprite = item.Sprite;
        }
    }
}
