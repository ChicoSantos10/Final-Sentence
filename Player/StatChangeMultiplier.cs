using System;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class StatChangeMultiplier
    {
        [SerializeField] float amount;
        
        public float Amount => amount;

        public StatChangeMultiplier(float amount)
        {
            this.amount = amount;
        }

        public static implicit operator float(StatChangeMultiplier multiplier) => multiplier.Amount;
    }
}
