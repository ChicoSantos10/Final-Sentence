using Managers;
using UnityEngine;

namespace Scriptable_Objects.Punishments
{
    [CreateAssetMenu(menuName = nameof(Punishment)+"/Lilith")]
    public class LilithPunishment : Punishment
    {
        [SerializeField] int newSoulsMultiplier = 2;
        [SerializeField] NumberVariable currentSoulsMultiplier;
        
        protected override void OnBegin()
        {
            currentSoulsMultiplier.Value = newSoulsMultiplier;
        }

        protected override void OnEnd()
        {
            currentSoulsMultiplier.Value = 1;
        }
    }
}
