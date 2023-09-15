using System;
using System.Collections.Generic;
using Player;
using UI.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Items
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(Equipment))]
    public class Equipment : InventoryItem
    {
        [Serializable]
        class EquipmentInfo
        {
            [SerializeField] Stat.StatType stat;
            [SerializeField] StatModifier modifier;

            public Stat.StatType Stat => stat;

            public StatModifier Modifier => modifier;
        }
        
        [FormerlySerializedAs("equipment")] [SerializeField] EquipmentInventory equipmentInventory;
        [SerializeField] EquipmentSlotType type;
        [SerializeField] EquipmentSlot preferredSlot; 
        [SerializeField, Range(1,2)] int amount = 1;
        [SerializeField] PlayerInfo player;
        [SerializeField] EquipmentInfo[] info;
        
        public override int StackMax { get; } = 1;
        
        public override void Equip(Inventory.ItemStack stack)
        {
            if (equipmentInventory.IsEquipped(stack, type, out EquipmentSlot eqSlot))
            {
                // TODO: Unequip the item
                Unequip();
                eqSlot.RemoveItem();
                
                return;
            }

            // TODO: 2-handed 
            if (equipmentInventory.GetSlot(preferredSlot, out EquipmentSlot slot))
            {
                if (slot.Stack == null)
                {
                    slot.SetItem(stack, CreateItem(slot));
                    OnEquip(stack);
                    return;
                }
            }
            
            EquipmentSlot[] slots = equipmentInventory.GetSpacesOfType(amount, type, out EquipmentSlot[] s) ? s : equipmentInventory.GetFirstSlots(amount, type);
            
            // TODO: Override effector so the item is correctly handled (e.g. Both hands should be holding a two handed weapon)
            GameObject item = CreateItem(slots[0]);

            foreach (EquipmentSlot equipmentSlot in slots)
            {
                equipmentSlot.Stack?.Item.Unequip();
                
                // equipmentSlot.Stack = this;
                
                equipmentSlot.SetItem(stack, item);
            }
            
            OnEquip(stack);
        }

        public override bool EquipToSlot(InventorySlot slot, Inventory.ItemStack stack)
        {
            throw new System.NotImplementedException();
        }

        void OnEquip(Inventory.ItemStack stack)
        {
            foreach (EquipmentInfo itemInfo in info)
            {
                player.Stats[itemInfo.Stat].AddModifier(itemInfo.Modifier);
            }
        }

        GameObject CreateItem(EquipmentSlot slot)
        {
            GameObject item = new GameObject(name);
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite;
            renderer.sortingLayerName = "Ground Elements";
            renderer.sortingOrder = 5;

            Transform place = FindChild(slot.Place, player.Player);
            item.transform.SetParent(place);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            return item;
        }

        Transform FindChild(string name, Transform parent)
        {
            Transform child = null;
 
            // Loop through top level
            foreach (Transform t in parent) 
            {
                if (t.name.Equals(name)) 
                {
                    child = t;
                    return child;
                }

                if (t.childCount <= 0) 
                    continue;
                
                child = FindChild(name, t);
                if (child)
                    return child;
            }
            
            Debug.LogError($"Transform named: {name} not found");
            return child;
        }

        public override void Unequip()
        {
            foreach (EquipmentInfo itemInfo in info)
            {
                player.Stats[itemInfo.Stat].RemoveModifier(itemInfo.Modifier);
            }
        }

        void OnValidate()
        {
            if (preferredSlot == null || preferredSlot.Type == type)
                return;
            
            Debug.LogError("Preferred slot does not match the item type. Removing it");
            preferredSlot = null;
        }
    }
}
