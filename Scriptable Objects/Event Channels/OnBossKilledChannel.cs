using Enemies;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects.Event_Channels
{
    [CreateAssetMenu(menuName = "Event Channels/" + nameof(OnBossKilledChannel))]
    public class OnBossKilledChannel : ScriptableObject, ISerializationCallbackReceiver
    {
        public event UnityAction<Boss> Event = delegate { };

        public void Invoke(Boss boss)
        {
            Event.Invoke(boss);
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
