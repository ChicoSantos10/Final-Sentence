using System;
using System.Collections.Generic;
using Craft;
using UnityEngine;

namespace Scriptable_Objects.Items.Recipes
{
    public abstract class Recipe : ScriptableObject
    {
        public List<Ingredient> ingredients;

        public abstract Item Item { get; }
        
        /// <summary>
        /// Crafts the object
        /// </summary>
        /// <returns>If it should close the craft menu</returns>
        public abstract bool Craft();
    }
}
