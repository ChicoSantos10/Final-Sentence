using System;
using Player;
using UI.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Items
{
    public abstract class InventoryItem : Item
    {
        [SerializeField] protected Inventory inventory;
        
        public abstract int StackMax { get; }
        
        public abstract void Equip(Inventory.ItemStack stack);
        public abstract bool EquipToSlot(InventorySlot slot, Inventory.ItemStack stack);
        public abstract void Unequip();
    }
}
