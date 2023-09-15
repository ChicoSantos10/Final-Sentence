using System;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class StatModifier 
    {
        public enum ModifierType
        {
            Flat, Mult
        }

        [SerializeField] int amount;
        [SerializeField] ModifierType type;

        public int Amount => amount;
        public ModifierType Type => type;
    }
}
