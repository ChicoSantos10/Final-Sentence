using System;
using System.Collections;
using Scriptable_Objects;
using Scriptable_Objects.CraftMenu;
using UI;
using UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Managers
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [SerializeField] InputReader input;
        [SerializeField] GameObject inventoryMenu, bag, hand, bagSlot, handSlot;
        [SerializeField] InventoryIcons icons;

        Animator _menuAnimator;
        static readonly int Close = Animator.StringToHash("close");

        void Awake()
        {
            _menuAnimator = inventoryMenu.GetComponent<Animator>();
        }

        void Start()
        {
            inventoryMenu.SetActive(true);
            
            //inventory.Create();
            
            // Create the bag
            for (int i = 0; i < inventory.Capacity; i++)
            {
                InventorySlot slot = Instantiate(bagSlot, bag.transform).GetComponentInChildren<InventorySlot>();
                slot.inventory = inventory;
                slot.icons = icons;
                slot.SetItem(inventory[i]);
            }

            // Create the hand
            // for (int i = 0; i < inventory.HandSize; i++)
            // {
            //     EquipmentSlotUI hs = Instantiate(handSlot, hand.transform).GetComponentInChildren<EquipmentSlotUI>();
            //     hs.inventory = inventory;
            //     hs.slot = i;
            //     hs.type = EquipmentSlotUI.SlotType.Consumables;
            // }
            
            inventoryMenu.SetActive(false);
        }

        void OnEnable()
        {
            inventory.OnItemAdded += OnItemAdded;
            inventory.OnItemRemoved += OnItemRemoved;
            inventory.OnInventoryRefresh += InventoryRefresh;
            
            input.OnOpenInventoryAction += OpenInventory;
        }

        void OnDisable()
        {
            inventory.OnItemAdded -= OnItemAdded;
            inventory.OnItemRemoved -= OnItemRemoved;            
            inventory.OnInventoryRefresh -= InventoryRefresh;

            input.OnOpenInventoryAction -= OpenInventory;
        }
        
        void InventoryRefresh()
        {
            // Create the bag
            for (int i = 0; i < inventory.Capacity; i++)
            {
                InventorySlot slot = GetSlot(i);
                slot.inventory = inventory;
                slot.SetItem(inventory[i]);
            }
        }
        
        void OpenInventory()
        {   
            if(!inventoryMenu.activeSelf)
                inventoryMenu.SetActive(true);
            else
                _menuAnimator.SetTrigger(Close);
        }

        void OnItemRemoved(Inventory.ItemStack stack)
        {
            GetSlot(stack.Position).RemoveSlot();
        }

        void OnItemAdded(Inventory.ItemStack stack)
        {
            GetSlot(stack.Position).SetItem(stack);
        }

        InventorySlot GetSlot(int position)
        {
            Transform child = bag.transform.GetChild(position);
            
            return child.GetComponentInChildren<InventorySlot>();
        }
    }
}
