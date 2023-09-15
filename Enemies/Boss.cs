using Scriptable_Objects.Event_Channels;
using Scriptable_Objects.Punishments;
using UnityEngine;

namespace Enemies
{
    public class Boss : Enemy
    {
        [SerializeField] Punishment punishment;
        [SerializeField] OnBossKilledChannel bossKilledChannel;
        
        public Punishment Punishment => punishment;

        protected override void Die()
        {
            bossKilledChannel.Invoke(this);
            
            base.Die();
        }
    }
}
