using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Items.Recipes
{
    [CreateAssetMenu(menuName = nameof(Items.Item) + "/" + nameof(BuildingRecipe))]
    public class BuildingRecipe : Recipe
    {
        [SerializeField] GameObject worldItem;
        [SerializeField] Building item;
        [SerializeField] InputReader input;
        [SerializeField] PlayerInfo info;
        [SerializeField, Tooltip("Whether this object should immediately construct(be placed)")] bool isInstant;
        
        public static UnityAction<Vector3> BuildingPlacedEvent = delegate {  };

        public override Item Item => item;

        public override bool Craft()
        {
            GameObject building = Instantiate(worldItem, info.Player);
            
            if (isInstant)
            {
                PlaceItem();
                return true;
            }
            
            input.OnSubmitAction += PlaceItem;
            
            void PlaceItem()
            {
                building.transform.SetParent(null, true);
                
                BuildingPlacedEvent.Invoke(building.transform.position);
            
                input.OnSubmitAction -= PlaceItem;
            }
            
            return true;
        }
    }
}
