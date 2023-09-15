using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Player
{
    [Serializable]
    public class Stat : ISerializationCallbackReceiver
    {
        public enum StatType
        {
            Attack,
            Defense,
        }
        
        [SerializeField] StatType type;
        public StatType Type => type;

        [SerializeField] int baseValue;

        [NonSerialized] List<StatModifier> _modifiers = new List<StatModifier>();

        public int Value => ComputeValue();
        
        int ComputeValue()
        {
            float currentValue = baseValue;

            foreach (StatModifier modifier in _modifiers)
            {
                currentValue += modifier.Type switch
                {
                    StatModifier.ModifierType.Flat => modifier.Amount,
                    StatModifier.ModifierType.Mult => baseValue * modifier.Amount,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            return Mathf.RoundToInt(currentValue);
        }

        public void AddModifier(StatModifier modifier)
        {
            if (!_modifiers.Contains(modifier))
                _modifiers.Add(modifier);
        }
        
        public void RemoveModifier(StatModifier modifier)
        {
            _modifiers.Remove(modifier);
        }

        public override string ToString()
        {
            return ComputeValue().ToString();
        }

        public static implicit operator int(Stat stat) => stat.Value;
        
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }

        public object Save()
        {
            return new SaveData(_modifiers);
        }

        public void Load(object data)
        {
            if (data is SaveData save)
            {
                _modifiers = save.Modifiers;
            }
        }

        [Serializable]
        struct SaveData
        {
            public readonly List<StatModifier> Modifiers;

            public SaveData(List<StatModifier> modifiers)
            {
                Modifiers = modifiers;
            }
        }
    }
}
