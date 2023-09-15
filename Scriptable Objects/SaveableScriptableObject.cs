using System;
using SaveData;
using UnityEditor;
using UnityEngine;

namespace Scriptable_Objects
{
    public abstract class SaveableScriptableObject : ScriptableObject, ISaveData
    {
        [SerializeField, HideInInspector] string id = string.Empty;
        public string Id
        {
            get
            {
                #if UNITY_EDITOR
                id = id.Length < 1 ? GUID.Generate().ToString() : id;
                #endif
                
                return id;
            }
        }

        public abstract object Save();

        public abstract void Load(object data);
    }
}
