using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class HandSlotHUD : MonoBehaviour
    {
        [SerializeField] Scriptable_Objects.Inventory inventory;
        [SerializeField] Image[] slots;

        void OnEnable()
        {
            inventory.OnAddToHand += Inventory_OnAddToHand;
            inventory.OnRemoveFromHand += Inventory_OnRemoveFromHand;
            inventory.OnInventoryRefresh += OnInventoryRefresh;
        }

        void OnDisable()
        {
            inventory.OnAddToHand -= Inventory_OnAddToHand;
            inventory.OnRemoveFromHand -= Inventory_OnRemoveFromHand;
            inventory.OnInventoryRefresh -= OnInventoryRefresh;
        }
        
        void OnInventoryRefresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (inventory.GetItemHand(i, out Scriptable_Objects.Inventory.ItemStack stack))
                    EnableSlot(stack, i);
                else
                    DisableSlot(i);
            }
        }

        void Inventory_OnAddToHand(Scriptable_Objects.Inventory.ItemStack stack, int slot)
        {
            EnableSlot(stack, slot);
        }

        void Inventory_OnRemoveFromHand(int slot)
        {
            DisableSlot(slot);
        }
        
        void EnableSlot(Scriptable_Objects.Inventory.ItemStack stack, int slot)
        {
            slots[slot].gameObject.SetActive(true);
            slots[slot].sprite = stack.Item.Sprite;
        }

        void DisableSlot(int slot)
        {
            slots[slot].gameObject.SetActive(false);
        }
    }
}
