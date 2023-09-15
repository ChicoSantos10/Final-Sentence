using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = nameof(ItemType))]
    public class ItemType : ScriptableObject
    {
        [SerializeField] Sprite sprite;

        public Sprite Sprite => sprite;
    }
}
