using Scriptable_Objects;
using UnityEngine;
using static Scriptable_Objects.Inventory;

namespace Player
{
    public class PlayerConsume : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [SerializeField] PlayerInfo playerInfo;
        [SerializeField] InputReader inputReader;

        void OnEnable()
        {
            inputReader.OnConsumeAction += InputReader_OnConsumeAction;
        }

        void OnDisable()
        {
            inputReader.OnConsumeAction -= InputReader_OnConsumeAction;
        }

        void InputReader_OnConsumeAction(int slot)
        {
            if (!inventory.GetItemHand(slot, out ItemStack stack))
                return;
            
            //Consumable consumable = stack.Item.InGameObject.GetComponent<Consumable>(); 

            (stack.Item as Scriptable_Objects.Items.Consumable)?.Consume(playerInfo);
            
            stack.Remove();
            
            if (stack.Quantity <= 0)
                inventory.RemoveFromHand(slot);
        }
    }
}
