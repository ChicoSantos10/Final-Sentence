using System;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects.Items;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = nameof(Inventory))]
    public class Inventory : SaveableScriptableObject
    {
        public event UnityAction<ItemStack> OnItemAdded = delegate {  };
        public event UnityAction<ItemStack> OnItemRemoved = delegate {  };
        public event UnityAction<ItemStack, int> OnAddToHand = delegate {  };
        public event UnityAction<int> OnRemoveFromHand = delegate {  };
        public event UnityAction OnInventoryRefresh = delegate { };

        [SerializeField] int capacity = 20;
        [SerializeField] int handSize = 3;
        //[Header("Item Types"), Tooltip("Which item types correspond to what")]
        //[SerializeField] ItemType[] consumables, equipment, buildings;
        ItemStack[] _hand;

        ItemStack[] _inventory;
    
        public ItemStack this[int index] => _inventory[index]; // Maybe not necessary??
         
        public int Capacity => capacity;

        public int HandSize => handSize;

        void OnEnable()
        {
            Create();
        }

        void Create()
        {
            _inventory = new ItemStack[capacity];

            for (int i = 0; i < _inventory.Length; i++)
            {
                _inventory[i] = new ItemStack(this, null, 0, i);
            }

            _hand = new ItemStack[handSize];
        }

        /// <summary>
        /// Checks if inventory has the required amount of a certain item
        /// </summary>
        /// <param name="item">The item to search</param>
        /// <param name="quantity">The amount to check</param>
        /// <returns></returns>
        public bool HasItem(InventoryItem item, int quantity = 0)
        {
            int found = 0;
            
            foreach (ItemStack i in _inventory)
            {
                if (item != i.Item) 
                    continue;
                
                found += i.Quantity;

                if (found >= quantity)
                    return true;
            }

            return false;
        }
        
        public void AddItem(InventoryItem item, int quantity)
        {
            if (quantity <= 0)
                return;

            AvailableStackInfo info = FindAvailableStacks(item, quantity);
            
            foreach (ItemStack stack in info.Stacks)
            {
                quantity = stack.Add(quantity);
                OnItemAdded.Invoke(stack);
            }

            // Still need to add items
            while (info.Leftover > 0)
            {
                if (!FindFirstEmptyStack(out ItemStack stack))
                    break; // TODO: No more available space in inventory. Needs to be dealt with 
                
                stack.Item = item;
                info.Leftover = stack.Add(info.Leftover);
                OnItemAdded.Invoke(stack);
            }
            
            

            // Check if it already has
            /*if (FindFirstStack(item, out ItemStack stack))
            {
                stack.Quantity += quantity;
                OnItemAdded.Invoke(stack);
            }
            else
            {
                int spot = FindNextFreePosition();
                stack = new ItemStack(this, item, quantity, spot); // TODO: Just change values instead
                _inventory[spot] = stack;
                OnItemAdded.Invoke(stack);
            }*/
        }

        public void RemoveItem(InventoryItem item, int quantity)
        {
            // if (!FindFirstStack(item, out ItemStack stack))
            //     return;
            
            while (FindFirstStack(item, out ItemStack stack) && quantity > 0)
            {
                quantity = stack.Remove(quantity); 
                
                if (stack.Quantity < 0)
                    RemoveItem(stack);
            }
        }

        public int GetAvailableAmount(InventoryItem item)
        {
            return _inventory.Where(stack => stack.Item == item).Sum(stack => stack.Quantity);
        }

        void RemoveItem(ItemStack stack)
        {
            _inventory[stack.Position].Item = null;
            
            OnItemRemoved.Invoke(stack);
        }

        public void AddItemToHand(ItemStack stack, int slot)
        {
            if (slot >= handSize)
            {
                Debug.LogError("Tried to add an item to a non existing hand slot");
                return;
            }
            
            _hand[slot] = stack;
            
            OnAddToHand.Invoke(stack, slot);
        }

        public void AddItemAvailableSlotHand(ItemStack stack)
        {
            for (int index = 0; index < _hand.Length; index++)
            {
                if (_hand[index] != null) 
                    continue;
                
                AddItemToHand(stack, index);
                return;
            }
        }

        public void RemoveFromHand(int slot)
        {
            _hand[slot] = null; // Empty Stack?
            
            OnRemoveFromHand.Invoke(slot);
        }

        public bool IsItemEquipped(ItemStack item, out int slot)
        {
            slot = -1;
            
            for (int i = 0; i < _hand.Length; i++)
            {
                if (_hand[i] != item) 
                    continue;
                
                slot = i;
                return true;
            }

            return false;
        }

        bool FindFirstStack(InventoryItem item, out ItemStack itemStack)
        {
            itemStack = null;
            
            foreach (ItemStack stack in _inventory)
            {
                if (stack.Item != item)
                    continue;
                
                itemStack = stack;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the necessary stacks that can hold the quantity
        /// </summary>
        /// <param name="item">The item to find the stacks of</param>
        /// <param name="amount">The amount needed available</param>
        /// <returns>Information about the stacks needed</returns>
        AvailableStackInfo FindAvailableStacks(InventoryItem item, int amount)
        {   
            List<ItemStack> availableStacks = new List<ItemStack>();

            int amountNeeded = amount;
            
            foreach (ItemStack stack in _inventory)
            {
                // Check to see if stack has intended item
                if (stack.Item != item)
                    continue;

                // Check the capacity left for this stack
                int capacityLeft = stack.GetCapacityLeft();

                if (capacityLeft <= 0) 
                    continue;
                
                availableStacks.Add(stack);
                
                // See if found all 
                amountNeeded -= capacityLeft;

                if (amountNeeded <= 0)
                    return new AvailableStackInfo(0, availableStacks.ToArray());
            }

            return new AvailableStackInfo(amountNeeded, availableStacks.ToArray());
        }

        bool FindFirstEmptyStack(out ItemStack itemStack)
        {
            foreach (ItemStack stack in _inventory)
            {
                if (stack.Item == null)
                {
                    itemStack = stack;
                    return true;
                }
            }

            itemStack = null;
            return false;
        }

        public void Swap(ItemStack stack1, ItemStack stack2)
        {
            int s2Pos = stack2.Position;
            
            _inventory[stack1.Position] = stack2;
            _inventory[stack1.Position].Position = stack1.Position;
            
            _inventory[s2Pos] = stack1;
            _inventory[s2Pos].Position = s2Pos;
        }

        public bool GetItemHand(int slot, out ItemStack stack)
        {
            if (slot <= handSize && _hand[slot] != null && _hand[slot].Item != null)
            {
                stack = _hand[slot];
                return true;
            }
            
            stack = ItemStack.EmptyStack;
            return false;
        }
        
        public class ItemStack
        {
            public static readonly ItemStack EmptyStack = new ItemStack(null, null, 0, -1);
            
            public event UnityAction OnQuantityChanged = delegate { };

            int _quantity;
            public int Quantity => _quantity;

            public InventoryItem Item { get; set; }
            
            public int Position { get; set; }
            
            readonly Inventory _inventory;

            public ItemStack(Inventory inventory, InventoryItem item, int quantity, int position)
            {
                _inventory = inventory;
                _quantity = quantity;
                Position = position;
                Item = item;
            }

            /// <summary>
            /// Adds the amount and returns the leftover
            /// </summary>
            /// <param name="amount"></param>
            /// <returns>The leftover after adding the amount</returns>
            public int Add(int amount)
            {
                if (amount < 0)
                {
                    Debug.LogWarning("Trying to add a negative number. Removing it instead");
                    Remove(-amount);
                    return 0;
                }
                
                int capacityLeft = GetCapacityLeft();

                int addAmount = 0;
                int leftover = 0;

                if (capacityLeft > amount)
                    addAmount = amount;
                else
                {
                    addAmount = capacityLeft;
                    leftover = amount - capacityLeft;
                }

                _quantity += addAmount;
                
                OnQuantityChanged.Invoke();
                
                return leftover;
            }

            /// <summary>
            /// Removes amount
            /// </summary>
            /// <param name="amount">The amount to remove</param>
            /// <returns>The leftover that wasn't removed</returns>
            public int Remove(int amount)
            {
                _quantity -= amount;
                int leftover = 0;
                
                if (_quantity <= 0)
                {
                    leftover = -_quantity;
                    _inventory.RemoveItem(this);
                }
                    
                OnQuantityChanged.Invoke();

                return leftover;
            }

            /// <summary>
            /// Removes one
            /// </summary>
            public void Remove()
            {
                _quantity--;
                
                if (_quantity <= 0) 
                    _inventory.RemoveItem(this);

                OnQuantityChanged.Invoke();
            }

            public int GetCapacityLeft()
            {
                return Item.StackMax - Quantity;
            }

            public object Save()
            {
                return new ItemStackSaveData(_quantity, Item.Id);
            }

            public void Load(object data)
            {
                if (data is ItemStackSaveData save)
                {
                    _quantity = save.quantity;
                    Item = (InventoryItem)Items.Item.ItemDatabase[save.item];
                }
                else
                {
                    Debug.LogError("Load Failed");
                }
            }
            
            [Serializable]
            struct ItemStackSaveData
            {
                public int quantity;
                public string item;

                public ItemStackSaveData(int quantity, string item)
                {
                    this.quantity = quantity;
                    this.item = item;
                }
            }
        }

        class AvailableStackInfo
        {
            public int Leftover;
            public readonly ItemStack[] Stacks;

            public AvailableStackInfo(int leftover, ItemStack[] stacks)
            {
                Leftover = leftover;
                Stacks = stacks;
            }
        }

        public override object Save()
        {
            Dictionary<int, object> inventoryData = _inventory.Where(stack => stack.Quantity > 0).ToDictionary(stack => stack.Position, stack => stack.Save());

            Dictionary<int, int> handData = new Dictionary<int, int>();
            for (int index = 0; index < _hand.Length; index++)
            {
                ItemStack stack = _hand[index];
                if (stack != null)
                    handData.Add(index, stack.Position);
            }

            return new SaveData(inventoryData, handData);
        }

        public override void Load(object data)
        {
            if (data is SaveData save)
            {
                // Clear the inventory
                Create();
                
                foreach (KeyValuePair<int, object> stack in save.InventoryData)
                {
                    _inventory[stack.Key].Load(stack.Value);
                }

                foreach (KeyValuePair<int, int> stack in save.HandData)
                {
                    _hand[stack.Key] = _inventory[stack.Value];
                }
                
                OnInventoryRefresh.Invoke();
            }
            else
            {
                Debug.Log("Load failed");
            }
        }

        [Serializable]
        struct SaveData
        {
            public Dictionary<int, object> InventoryData;
            public Dictionary<int, int> HandData;

            public SaveData(Dictionary<int, object> inventoryData, Dictionary<int, int> handData)
            {
                InventoryData = inventoryData;
                HandData = handData;
            }
        }
    }
}
