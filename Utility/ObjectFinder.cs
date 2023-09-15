using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utility
{
    public static class ObjectFinder
    {
#if UNITY_EDITOR
        /// <summary>
        /// All scriptable objects of type
        /// </summary>
        /// <param name="type">The type of scriptable object to get</param>
        /// <returns></returns>
        public static ScriptableObject[] GetElementsScriptableObjects(Type type)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{type}");

            return guids.Select(path => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), type) as ScriptableObject).ToArray();
        }
#endif
    }
}
