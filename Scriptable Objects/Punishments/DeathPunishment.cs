using Managers;
using Player;
using UnityEngine;

namespace Scriptable_Objects.Punishments
{
    [CreateAssetMenu(menuName = nameof(Punishment) + "/" + nameof(DeathPunishment))]
    public class DeathPunishment : Punishment
    {
        [SerializeField] StatChangeMultiplier hpMult;
        [SerializeField] PlayerInfo player;
        
        protected override void OnBegin()
        {
            player.Stats[VariableStat.StatType.Hp].AddMult(hpMult);
        }

        protected override void OnEnd()
        {
            player.Stats[VariableStat.StatType.Hp].RemoveMult(hpMult);            
        }
    }
}
