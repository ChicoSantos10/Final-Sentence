using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects.Event_Channels
{
    [CreateAssetMenu(menuName = "Event Channels/" + nameof(EventChannel))]
    public class EventChannel : ScriptableObject, ISerializationCallbackReceiver
    {
        public event UnityAction Event = delegate { };

        public void Invoke()
        {
            Event.Invoke();
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            Event = delegate { };
        }
    }
}
