using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects;
using Scriptable_Objects.CraftMenu;
using Scriptable_Objects.Items;
using Scriptable_Objects.Items.Recipes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Craft
{
    public class CraftManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] InputReader input;

        [Header("Menu")] 
        [SerializeField] GameObject craftMenu;
        [SerializeField] Button itemTypeButton;
        [SerializeField] Transform menuOptions;
        [SerializeField] Color disabledColor = Color.gray;

        [Header("Item Display")] 
        [SerializeField] Image itemSprite;
        [SerializeField] Transform resourceLocation;
        [SerializeField] GameObject resourceUI;

        [Header("Player")]
        [SerializeField] Inventory playerInventory;

        [SerializeField] CraftMenuOption[] menus;
        
        //readonly Dictionary<ItemType, List<Item>> _items = new Dictionary<ItemType, List<Item>>();
        CraftMenuOption _selectedType;
        int _selectedTypeIndex = 0;
        int _itemIndex = 0;
        
        Animator _animator;
        static readonly int Close = Animator.StringToHash("close");

        void Awake()
        {
            input.OnOpenCraftMenuAction += OpenMenu;
            
            /*foreach (Item item in items)
            {
                ItemType type = item.Type;
                
                if (_items.ContainsKey(type))
                {
                    _items[type].Add(item);
                }
                else
                {
                    _items.Add(type, new List<Item> {item});
                }
            }
            
            // Display all item types
            foreach (ItemType itemType in itemTypes)
            {
                Button b = Instantiate(itemTypeButton, menuOptions);
                b.gameObject.name = itemType.name;
                b.GetComponent<Image>().sprite = itemType.Sprite;
                
                b.onClick.AddListener(() =>
                {
                    _selectedType = itemType;
    
                    DisplayItemType();
                    
                    //b.gameObject.GetComponent<ItemTypeMenu>().DisplayItemType(_items[itemType]);

                    //b.transform.GetChild(0).gameObject.SetActive(true);

                    //b.gameObject.GetComponent<ItemTypeMenu>().CreateMenu(_items[itemType]);

                    //OpenMenu(b.gameObject.transform.Find("Content"), itemType);
                });
            }*/

            foreach (CraftMenuOption menu in menus)
            {
                Button b = Instantiate(itemTypeButton, menuOptions);
                b.gameObject.name = menu.name;
                Image image = b.GetComponent<Image>();
                image.sprite = menu.Sprite;
                image.color = disabledColor;
                
                b.onClick.AddListener(() =>
                {
                    _selectedType = menu;
                    ResetColors();
                    image.color = Color.white;
    
                    DisplayItemType();
                });
            }

            _itemIndex = 0;
            _selectedType = menus[0];
            //DisplayItemType();
            menuOptions.GetChild(0).GetComponent<Button>().onClick.Invoke();

            _animator = craftMenu.GetComponent<Animator>();
            
            DisableMenu();

            // foreach (ItemType type in _items.Keys)
            // {
            //     print(type);
            //     
            //     foreach (Item item in _items[type])
            //     {
            //         print(item);
            //     }
            // }
        }

        void ResetColors()
        {
            foreach (Transform menuOption in menuOptions)
            {
                menuOption.GetComponent<Image>().color = disabledColor;
            }
        }

        void OpenMenu()
        {
            if (craftMenu.activeSelf)
                DisableMenu();
            else
                EnableMenu();

            // TODO: Listen to craft input
        }

        void DisplayItemType(int position = 0)
        {
            foreach (Transform children in resourceLocation)
            {
                Destroy(children.gameObject);
            }

            //Item current = _items[_selectedType][position];
            Recipe current = _selectedType.recipes[position];
            
            // Change sprite
            itemSprite.sprite = current.Item.Sprite;

            // Loop through recipe (resources needed)
            foreach (Ingredient ingredient in current.ingredients)
            {
                // Change sprite and amount needed
                GameObject go = Instantiate(resourceUI, resourceLocation);

                go.GetComponent<Image>().sprite = ingredient.Item.Sprite;
                int amountInventory = playerInventory.GetAvailableAmount(ingredient.Item);
                TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
                 text.text = $"{ingredient.Quantity}/{amountInventory}";
                
                if (amountInventory < ingredient.Quantity)
                    text.color = Color.red;
            }
        }

        public void MoveNextItem(int amount)
        {
            int itemCount = _selectedType.recipes.Length;
            
            _itemIndex += amount;

            if (_itemIndex >= 0)
                _itemIndex %= itemCount;
            else
                _itemIndex = itemCount + _itemIndex;
            
            DisplayItemType(_itemIndex);
        }

        void MoveNextItem(Vector2 dir)
        {
            MoveNextItem((int) dir.x);

            // Vertical navigation
            if (dir.y == 0) 
                return;
            
            int itemCount = menus.Length;

            _selectedTypeIndex -= (int) dir.y;

            if (_selectedTypeIndex >= 0)
                _selectedTypeIndex %= itemCount;
            else
                _selectedTypeIndex = itemCount + _selectedTypeIndex;

            _selectedType = menus[_selectedTypeIndex];
            
            //DisplayItemType();
            menuOptions.GetChild(_selectedTypeIndex).GetComponent<Button>().onClick.Invoke();
        }

        void CraftItem()
        {
            Recipe item = _selectedType.recipes[_itemIndex];
            
            print($"Item to craft: {item.name}");
            
            // Check if player has enough resources
            if (item.ingredients.All(ingredient => playerInventory.HasItem(ingredient.Item, ingredient.Quantity)))
            {
                foreach (Ingredient ingredient in item.ingredients)
                {
                    playerInventory.RemoveItem(ingredient.Item, ingredient.Quantity);
                }

                if (item.Craft())
                    DisableMenu();
                
                // if (addToInventoryTypes.Contains(item.Type))
                //     inventory.AddItem(item, 1);
                // else if (placeOnMapTypes.Contains(item.Type))
                //     StartCoroutine(TryPlaceItem(item.InGameObject));
                // else
                //     Debug.LogWarning($"Cannot do anything with this type: {item.Type}");
            }
            else
            {
                print("Insufficient resources");
            }
        }

        /*void OpenMenu(Transform parent, ItemType type)
        {
            foreach (InventoryItem item in _items[type])
            {
                GameObject go = new GameObject(item.name)
                {
                    transform =
                    {
                        parent = parent
                    }
                };
                print(go.transform.parent);
                Button b = go.AddComponent<Button>(); 
                go.AddComponent<Image>().sprite = item.Sprite;
            }
        }*/

        void EnableMenu()
        {
            craftMenu.SetActive(true);
            
            input.OnNavigationAction += MoveNextItem;
            input.OnSubmitAction += CraftItem;
            input.DisableGameplay();
        }

        void DisableMenu()
        {
            //craftMenu.SetActive(false);
            _animator.SetTrigger(Close);
            
            input.OnNavigationAction -= MoveNextItem;
            input.OnSubmitAction -= CraftItem;
            input.EnableGameplay();
        }

        void OnDisable()
        {
            input.OnOpenCraftMenuAction -= OpenMenu;
        }
    }
}
