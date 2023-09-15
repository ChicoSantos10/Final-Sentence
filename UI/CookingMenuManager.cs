using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Craft;
using Managers;
using Scriptable_Objects;
using Scriptable_Objects.Items;
using Scriptable_Objects.Items.Recipes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(EventTrigger))]
    public class CookingMenuManager : MonoBehaviour
    {
        // TODO: Scroll
        
        // Display item info
        
        // Create the menu

        [Header("Menu info")]
        [SerializeField] RectTransform content; // Place to create the items in the scroll view
        [SerializeField] GameObject recipeItem; // The prefab of item to add
        [SerializeField] GameObject itemDetails; // The game object holding the details for the recipe
        [SerializeField] GameObject benefitInfo;
        [SerializeField] Transform benefitContent;
        [SerializeField] GameObject costImage;
        [SerializeField] Transform costContent;
        [SerializeField] ConsumableRecipe[] recipes;

        [Header("Input"), SerializeField] InputReader input;
        [SerializeField] Scriptable_Objects.Inventory playerInventory;

        readonly Dictionary<GameObject, ConsumableRecipe> _consumables = new Dictionary<GameObject, ConsumableRecipe>();
        EventTrigger _eventTrigger;
        GameObject _selected;
        
        // Scrolling variables
        float _step, _top, _bot; 

        void Awake()
        {
            int index = 0;
            
            // Initialize
            foreach (ConsumableRecipe recipe in recipes)
            {
                GameObject go = Instantiate(recipeItem, content);

                go.GetComponentsInChildren<Image>()[1].sprite = recipe.Item.Sprite;
                go.name = index++.ToString(); 
                
                _consumables.Add(go, recipe);
            }
            
            // Set the event listeners
            _eventTrigger = GetComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener(data => OnPointerClick((PointerEventData)data));
            _eventTrigger.triggers.Add(entry);
        }

        void Start()
        {
             int size = recipes.Length;
             
             GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
             _step = grid.cellSize.y + grid.spacing.y;
             _bot = (size - 1) * 0.5f * _step;
             _top = -_bot;
             
             // Set the position to the top
             ScrollTo(_top);
        }
        
        void OnEnable()
        {
            input.OnNavigationAction += OnNavigationAction;
            input.OnSubmitAction += Craft;

            input.DisableGameplay();
        }
        
        void OnDisable()
        {
            input.OnNavigationAction -= OnNavigationAction;
            input.OnSubmitAction -= Craft;
            
            input.EnableGameplay();
        }

        void Craft()
        {
            ConsumableRecipe recipe = _consumables[_selected];

            if (!recipe.ingredients.All(ingredient => playerInventory.HasItem(ingredient.Item, ingredient.Quantity))) 
                return;
            
            foreach (Ingredient ingredient in recipe.ingredients)
            {
                playerInventory.RemoveItem(ingredient.Item, ingredient.Quantity);
            }

            recipe.Craft();
        }
        
        void OnNavigationAction(Vector2 direction)
        {
            if (direction.y > 0)
                PageUp();
            else
                PageDown();
        }


        void ShowDetails()
        {   
            // Clear the current displayed by destroying all children
            CleanDetails();
            
            // Show modifiers
            foreach (Consumable.ItemInfo info in ((Consumable) _consumables[_selected].Item).Info)
            {
                GameObject go = Instantiate(benefitInfo, benefitContent);
                go.GetComponentInChildren<TextMeshProUGUI>().text = $"{info.Stat}: {info.Amount}";
            }
            
            // Show cost
            foreach (Ingredient ingredient in _consumables[_selected].ingredients)
            {
                GameObject obj = Instantiate(costImage, costContent);

                obj.GetComponentsInChildren<Image>()[1].sprite = ingredient.Item.Sprite;
                obj.GetComponentInChildren<TextMeshProUGUI>().text = $"x{ingredient.Quantity}";
            }
        }

        void CleanDetails()
        {
            Clean(benefitContent);
            Clean(costContent);

            void Clean(Transform transform)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        void OnPointerClick(PointerEventData eventData)
        {
            _selected = eventData.pointerEnter;
        }

        /// <summary>
        /// Scrolls to a place within top and bot
        /// </summary>
        /// <param name="value">The place to move to</param>
        void ScrollTo(float value)
        {
            Vector2 newPos = content.anchoredPosition;
            newPos.y = Mathf.Clamp(value, _top, _bot);

            content.anchoredPosition = newPos;
            
            OnScroll();
        }

        /// <summary>
        /// Scrolls by an amount
        /// </summary>
        /// <param name="amount"></param>
        void Scroll(float amount)
        {
            ScrollTo(content.anchoredPosition.y + amount);
        }
        
        void PageDown()
        {
            //StartCoroutine(Scroll(scroller.value - _step));
            Scroll(_step);
        }
        
        void PageUp()
        {
            //StartCoroutine(Scroll(scroller.value + _step));
            Scroll(-_step);
        }

        int GetObjectIndex()
        {
            // t = 0 when content position is at _top, 1 when is at _bot
            float totalDistance = _bot * 2;
            float t = (content.anchoredPosition.y + _bot) / totalDistance; 
            
            return Mathf.RoundToInt(Mathf.Lerp(0, recipes.Length - 1, t));
        }

        void OnScroll()
        {
            // Check if selected object is different to show details
            GameObject obj = content.GetChild(GetObjectIndex()).gameObject;

            if (_selected == obj)
                return;

            _selected = obj;
            ShowDetails();
        }

        // void OnValidate()
        // {
        //     foreach (ConsumableRecipe recipe in recipes)
        //     {
        //         if (recipes.Contains(recipe))
        //             Debug.LogError($"Duplicate found: {recipe} in {gameObject} {this}");
        //     }
        // }
    }
}
