using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Items.Recipes
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(EquipmentRecipe))]
    public class EquipmentRecipe : InventoryItemRecipe
    {
        [SerializeField] Equipment equipment;

        public override Item Item => equipment;
    }
}
