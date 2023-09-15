using System;
using System.Collections.Generic;
using Craft;
using UnityEditor;
using UnityEngine;

namespace Scriptable_Objects.Items
{
    public abstract class Item : ScriptableObject
    {
        //[SerializeField] ItemType type;
        [SerializeField] Sprite sprite;
        //[SerializeField] GameObject inGameObject;
        //[SerializeField] Recipe recipe;

        //public ItemType Type => type;

        public Sprite Sprite => sprite;

        //public GameObject InGameObject => inGameObject;

        //public Recipe Recipe => recipe;

        /// <summary>
        /// Crafts the object
        /// </summary>
        /// <returns>If it should close the craft menu</returns>
        //public abstract bool Craft();
        
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

        public static Dictionary<string, Item> ItemDatabase = new Dictionary<string, Item>();

        void OnEnable()
        {
            if (!ItemDatabase.TryGetValue(Id, out _))
                ItemDatabase.Add(Id, this);
        }
    }
}
