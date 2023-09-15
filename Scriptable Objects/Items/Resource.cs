using UI.Inventory;
using UnityEngine;

namespace Scriptable_Objects.Items
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(Resource))]
    public class Resource : InventoryItem
    {
        public override int StackMax => 50;

        // TODO: Resource probably shouldn't be an inventory item since it doesnt make sense to equip it
        
        public override void Equip(Inventory.ItemStack stack)
        {
        }

        public override bool EquipToSlot(InventorySlot slot, Inventory.ItemStack stack)
        {
            return false;
        }

        public override void Unequip()
        {
        }
    }
}
