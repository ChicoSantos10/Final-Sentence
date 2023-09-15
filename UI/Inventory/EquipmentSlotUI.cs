using System;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class EquipmentSlotUI : InventorySlot
    {
        public enum SlotType
        {
            Consumables,
            Equipment
        }
        
        public int slot;
        public SlotType type;

        public override void OnDrop(PointerEventData eventData)
        {
            Scriptable_Objects.Inventory.ItemStack stack = eventData.pointerDrag.GetComponent<InventorySlot>()?.GetItem();
            
            if (stack != null && stack.Item.EquipToSlot(this, stack))
            {
                SetItem(stack);
            }
        }
    }
}
