using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI.ProceduralMap
{
    [CreateAssetMenu(menuName = "Data/" + nameof(MapSettings))]
    public class MapSettings : ScriptableObject
    {
        [SerializeField] MapSize mapSize;
        
        [field:NonSerialized] public MapSize MapSize { get; set; }
        
        /// <summary>
        /// The random seed to use
        /// </summary>
        [field:NonSerialized] public int Seed { get; set; }

        void OnEnable()
        {
            Seed = Random.Range(0, int.Parse(MaxSeed));
            MapSize = mapSize;
        }

        static string MaxSeed = "1000000000";
        public static int MaxDigitString => MaxSeed.Length - 1;
    }

    public enum MapSize
    {
        Small, Medium, Large
    }
}
