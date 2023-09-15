using System;
using System.Collections.Generic;
using Scriptable_Objects.Items;
using UnityEngine;

namespace Scriptable_Objects.CraftMenu
{
    [CreateAssetMenu(menuName = nameof(Item) + "/" + "Inventory icons")]
    public class InventoryIcons : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] Sprite consumable;
        [SerializeField] Sprite equip;
        [SerializeField] Sprite resource;

        [NonSerialized] public Dictionary<Type, Sprite> Sprites;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            Sprites = new Dictionary<Type, Sprite>()
            {
                {typeof(Consumable), consumable},
                {typeof(Equipment), equip},
                {typeof(Resource), resource}
            };
        }
    }
}
