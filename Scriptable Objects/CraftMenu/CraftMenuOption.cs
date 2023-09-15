using Scriptable_Objects.Items;
using Scriptable_Objects.Items.Recipes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.CraftMenu
{
    [CreateAssetMenu(menuName = "Craft/" + nameof(CraftMenuOption))]
    public class CraftMenuOption : ScriptableObject
    {
        [SerializeField] Sprite sprite;

        public Sprite Sprite => sprite;
        
        [FormerlySerializedAs("items")] public Recipe[] recipes;
    }
}
