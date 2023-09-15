using System;
using System.Collections;
using Scriptable_Objects;
using Scriptable_Objects.CraftMenu;
using Scriptable_Objects.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    // TODO: Use input reader
    public class InventorySlot : EventTrigger
    {
        public Scriptable_Objects.Inventory inventory;
        public InventoryIcons icons;

        Scriptable_Objects.Inventory.ItemStack _stack = Scriptable_Objects.Inventory.ItemStack.EmptyStack;
        Image _image;
        Image _icon;
        TextMeshProUGUI _text;

        void Awake()
        {
            Image[] images = GetComponentsInChildren<Image>();
            _image = images[0];
            _icon = images[1];
            _icon.enabled = false;
;           _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        void OnDestroy()
        {
            _stack.OnQuantityChanged -= UpdateQuantity;
        }

        public void SetItem(Scriptable_Objects.Inventory.ItemStack stack)
        {
            _stack.OnQuantityChanged -= UpdateQuantity;
            
            _stack = stack;
            
            _stack.OnQuantityChanged += UpdateQuantity;
            
            if (stack.Quantity <= 0)
            {
                DisableStack();
                return;
            }
            
            UpdateQuantity();

            _image.enabled = true;
            _image.sprite = stack.Item.Sprite;

            _icon.enabled = true;
            _icon.sprite = icons.Sprites[stack.Item.GetType()];
        }

        void DisableStack()
        {
            _image.sprite = null;
            _image.enabled = false;

            _icon.enabled = false;
            
            _text.text = "";
        }

        void UpdateQuantity()
        {
            _text.text = _stack.Quantity.ToString();
        }

        public void RemoveSlot()
        {
            _stack.OnQuantityChanged -= UpdateQuantity;
            
            DisableStack();
        }

        public Scriptable_Objects.Inventory.ItemStack GetItem()
        {
            return _stack;
        }
        
        public override void OnDrop(PointerEventData eventData)
        {
            InventorySlot itemDropped = eventData.pointerDrag.GetComponent<InventorySlot>();
            
            // Hand to inventory
            // EquipmentSlotUI equipment = itemDropped as EquipmentSlotUI;;
            // if (equipment != null)
            // {
            //     inventory.RemoveFromHand(equipment.slot);
            //     equipment.RemoveSlot();
            //     return;
            // }

            inventory.Swap(itemDropped._stack, _stack);

            Scriptable_Objects.Inventory.ItemStack temp = _stack;
            SetItem(itemDropped._stack);
            itemDropped.SetItem(temp);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            
            GameObject obj = eventData.pointerPress;
            
            Scriptable_Objects.Inventory.ItemStack stack = obj.GetComponent<InventorySlot>()?.GetItem();
            
            Debug.Log("Click");
            
            stack?.Item.Equip(stack);
        }
        
        // Hand -> Inventory = Remove from hand
        // Inventory -> Inventory = Swap
        // Inventory -> Hand = Add to hand
    }
}
