using System;
using System.Collections;
using System.Linq;
using Player;
using UI.Inventory;
using UnityEngine;

namespace Scriptable_Objects.Items
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(Consumable))]
    public class Consumable : InventoryItem
    {
        [Serializable]
        public class ItemInfo
        {
            [SerializeField] VariableStat.StatType stat;
            [SerializeField] int amount;

            public VariableStat.StatType Stat => stat;

            public int Amount => amount;
        }
        
        [SerializeField] ItemInfo[] info;

        /// <summary>
        /// Gets a copy of the item info array
        /// </summary>
        public ItemInfo[] Info => info.ToArray();

        public override int StackMax => 66;

        // TODO: Bonus effects
        
        public override void Equip(Inventory.ItemStack stack)
        {
            if (inventory.IsItemEquipped(stack, out int slot))
                inventory.RemoveFromHand(slot);
            else
                inventory.AddItemAvailableSlotHand(stack);
        }

        public override bool EquipToSlot(InventorySlot slot, Inventory.ItemStack stack)
        {
            EquipmentSlotUI equipment = slot as EquipmentSlotUI;
            
            if (equipment == null)
                return false;

            inventory.AddItemToHand(stack, equipment.slot);            
            
            return true;
        }

        public override void Unequip()
        {
            throw new NotImplementedException();
        }

        public void Consume(PlayerInfo player)
        {
            // player.Hp.Recover(hp);
            // player.Stamina.Recover(stamina);
            // player.Hunger.Recover(hunger);

            foreach (ItemInfo recoverInfo in info)
            {
                player.Stats[recoverInfo.Stat].Recover(recoverInfo.Amount);
            }
        }
    }
}
