using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Player
{
    [Serializable]
    public class VariableStat : ISerializationCallbackReceiver
    {
        public enum StatType
        {
            Hp,
            Stamina, 
            Hunger
        }

        [SerializeField] StatType type;
        public StatType Type => type;
        
        [SerializeField] int maxValue; 
        float _currentValue;

        [NonSerialized] List<StatChangeMultiplier> _multipliers = new List<StatChangeMultiplier>();

        public event UnityAction<StatType, float> ValueChangedEvent = delegate {  };

        public int MaxValue => maxValue;

        public float CurrentValue => _currentValue;

        public void Recover(float amount)
        {
            float newValue = _currentValue +
                             _multipliers.Aggregate(amount, (current, multiplier) => current * multiplier);
            
            _currentValue = Mathf.Clamp(newValue, 0, maxValue);
            
            ValueChangedEvent.Invoke(type, _currentValue);
        }

        public void Reduce(float amount)
        {
            _currentValue = Mathf.Clamp(_currentValue - amount, 0, maxValue);
            
            ValueChangedEvent.Invoke(type, _currentValue);
        }

        public void AddMult(StatChangeMultiplier multiplier)
        {
            _multipliers.Add(multiplier);
        }

        public void RemoveMult(StatChangeMultiplier multiplier)
        {
            _multipliers.Remove(multiplier);
        }

        public void Reset() => _currentValue = maxValue;
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            Reset();
        }

        public override string ToString()
        {
            return $"{Mathf.Ceil(_currentValue)}/{maxValue}";
        }
        
        public static implicit operator float(VariableStat variableStat) => variableStat._currentValue;

        public object Save()
        {
            return new SaveData(_multipliers, _currentValue);
        }

        public void Load(object data)
        {
            if (data is SaveData save)
            {
                _multipliers = save.Multipliers;
                _currentValue = save.CurrentValue;
            }
            else
            {
                Debug.LogError("Load Failed");
            }
        }

        [Serializable]
        struct SaveData
        {
            public readonly List<StatChangeMultiplier> Multipliers;
            public readonly float CurrentValue;

            public SaveData(List<StatChangeMultiplier> multipliers, float currentValue)
            {
                Multipliers = multipliers;
                CurrentValue = currentValue;
            }
        }
    }
}
