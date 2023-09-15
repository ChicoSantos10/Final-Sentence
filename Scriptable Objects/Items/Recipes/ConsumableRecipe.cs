using UI.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Items.Recipes
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + nameof(ConsumableRecipe))]
    public class ConsumableRecipe : InventoryItemRecipe
    {
        [SerializeField] Consumable consumable;

        public override Item Item => consumable;
    }
}
