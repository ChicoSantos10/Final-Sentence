using System;
using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = "Variables/" + nameof(NumberVariable))]
    public class NumberVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] bool isInt;
        [SerializeField] float value;

        public float Value { get; set; }

        public static implicit operator float(NumberVariable variable) => variable.Value; 
        
        public void OnBeforeSerialize()
        {
            if (isInt)
                value = (int) value;
        }

        public void OnAfterDeserialize()
        {
            Value = isInt ? (int) value : value;
        }
    }
}
