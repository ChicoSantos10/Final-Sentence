using System.Collections.Generic;
using Scriptable_Objects;
using Scriptable_Objects.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Craft
{
    public class ItemTypeMenu : MonoBehaviour
    {
        [SerializeField] Transform itemPlace;
        [SerializeField] GameObject itemButton;

        public void CreateMenu(IEnumerable<InventoryItem> items)
        {
            foreach (InventoryItem item in items)
            {
                GameObject go = Instantiate(itemButton, itemPlace);

                go.name = item.name;
                
                go.GetComponent<Image>().sprite = item.Sprite;
                
                //go.GetComponent<Button>().onClick.AddListener(() => Instantiate(item.InGameObject));
            }
        }

        
    }
}
