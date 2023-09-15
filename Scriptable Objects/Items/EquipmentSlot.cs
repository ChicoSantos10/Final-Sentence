using System;
using UnityEngine;

namespace Scriptable_Objects.Items
{
    [CreateAssetMenu(menuName = "Equipment/" + nameof(EquipmentSlot))]
    public class EquipmentSlot : ScriptableObject, ISerializationCallbackReceiver
    {
        //[SerializeField] Equipment startingItem;
        [SerializeField] EquipmentSlotType type;
        [SerializeField, Tooltip("Where to place the item")] string objectPlace;

        [NonSerialized] Inventory.ItemStack _stack;
        [NonSerialized] GameObject _item;

        public EquipmentSlotType Type => type;

        public Inventory.ItemStack Stack => _stack;
        
        public string Place => objectPlace;

        public void SetItem(Inventory.ItemStack stack, GameObject item)
        {
            if (_item != null)
                Destroy(_item);
            
            _stack = stack;
            _item = item;
        }

        public void RemoveItem()
        {
            if (_item == null)
                return;
            
            Destroy(_item);
            _stack = null;
        }

        // public override bool Equals(object other)
        // {
        //     if (other == null || this == null)
        //         return false;
        //
        //     return name == (other as EquipmentSlot)?.name;
        // }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            //Item = startingItem;
        }
    }
}
