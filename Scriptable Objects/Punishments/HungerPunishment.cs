using Player;
using UnityEngine;

namespace Scriptable_Objects.Punishments
{
    [CreateAssetMenu(menuName = nameof(Punishment) + "/" + nameof(HungerPunishment))]
    public class HungerPunishment : Punishment
    {
        [SerializeField] StatChangeMultiplier hungerMult;
        [SerializeField] PlayerInfo player;
        
        protected override void OnBegin()
        {
            player.Stats[VariableStat.StatType.Hunger].AddMult(hungerMult);
        }

        protected override void OnEnd()
        {
            player.Stats[VariableStat.StatType.Hunger].RemoveMult(hungerMult);            
        }
    }
}
