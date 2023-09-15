using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using UnityEngine;

namespace Craft
{
    public class MenuInteractable : Interactable
    {
        [SerializeField] OpenMenuChannel menuChannel;
        
        protected override void OnInteractActionCompleted()
        {
            // Open menu
            menuChannel.Open();
            
            input.OnPreviousAction += OnMenuClose;
            
            // TODO: Maybe disable gameplay input
            
            // Disables the interaction UI
            EnableInteractionUI(false);
        }

        void OnMenuClose()
        {
            menuChannel.Close();

            input.OnPreviousAction -= OnMenuClose;
            
            EnableInteractionUI(true);
        }
    }
}
