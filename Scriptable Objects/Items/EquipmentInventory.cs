using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Scriptable_Objects.Items
{
    [CreateAssetMenu(menuName = "Equipment/" + nameof(EquipmentInventory))]
    public class EquipmentInventory : ScriptableObject
    {
        [SerializeField] List<EquipmentSlot> slots = new List<EquipmentSlot>();

        readonly Dictionary<EquipmentSlotType, List<EquipmentSlot>> _slotsByType =
            new Dictionary<EquipmentSlotType, List<EquipmentSlot>>();

        void OnEnable()
        {
            
#if UNITY_EDITOR

            ScriptableObject[] equipSlots = ObjectFinder.GetElementsScriptableObjects(typeof(EquipmentSlot));

            slots = new List<EquipmentSlot>();
            
            foreach (ScriptableObject slot in equipSlots)
            {
                slots.Add((EquipmentSlot) slot);
            }

#endif

            foreach (EquipmentSlot slot in slots)
            {
                if (_slotsByType.TryGetValue(slot.Type, out List<EquipmentSlot> s)) 
                    s.Add(Instantiate(slot));
                else
                    _slotsByType.Add(slot.Type, new List<EquipmentSlot> {Instantiate(slot)});
            }

        }

        public bool GetSpacesOfType(int amount, EquipmentSlotType type, out EquipmentSlot[] slots)
        {
            slots = new EquipmentSlot[amount];
            
            if (amount < 1)
            {
                Debug.LogWarning("Trying to check space smaller than 1");
                return false;
            }
            
            int index = 0;
            
            foreach (EquipmentSlot slot in _slotsByType[type].Where(slot => slot.Stack == null))
            {
                slots[index++] = slot;

                if (index >= amount)
                    return true;
            }

            return false;
        }

        public EquipmentSlot[] GetFirstSlots(int amount, EquipmentSlotType type)
        {
            // List<EquipmentSlot> tempSlots = new List<EquipmentSlot>();
            //
            // for (int i = 0; i < amount; i++)
            // {
            //     EquipmentSlot slot = slotsByType[type][i];
            //     tempSlots.Add(slot);
            // }

            return _slotsByType[type].GetRange(0, amount).ToArray();

            //return tempSlots.ToArray();
        }

        public bool IsEquipped(Inventory.ItemStack stack, EquipmentSlotType type, out EquipmentSlot slot)
        {
            foreach (EquipmentSlot s in _slotsByType[type].Where(s => s.Stack == stack))
            {
                slot = s;
                return true;
            }

            slot = null;
            return false;
        }

        public bool GetSlot(EquipmentSlot slot, out EquipmentSlot foundSlot)
        {
            foreach (EquipmentSlot equipmentSlot in _slotsByType[slot.Type]
                .Where(equipmentSlot =>
                {
                    string cloneName = equipmentSlot.name.Substring(0, slot.name.Length);
                    return cloneName == slot.name;
                }))
            {
                Debug.Log("Found slot");
                foundSlot = equipmentSlot;
                return true;
            }
            
            Debug.Log($"Slot not found {slot}");

            foundSlot = null;
            return false;
        }
    }
}
