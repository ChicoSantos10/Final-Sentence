using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects.Event_Channels
{
    [CreateAssetMenu(menuName = "Event Channels/" + nameof(OpenMenuChannel))]
    public class OpenMenuChannel : ScriptableObject
    {
        public GameObject menu;
        public event UnityAction OnMenuOpen = delegate { };
        public event UnityAction OnMenuClose = delegate { };
        
        public void Open()
        {
            OnMenuOpen.Invoke();
        }
        
        public void Close()
        {
            OnMenuClose.Invoke();
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            OnMenuOpen = delegate { };
            OnMenuClose = delegate {  };
        }
    }
}
