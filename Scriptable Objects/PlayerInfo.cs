using System;
using System.Collections.Generic;
using Player;
using SaveData;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = "Data/" + nameof(PlayerInfo))]
    public class PlayerInfo : SaveableScriptableObject, ISerializationCallbackReceiver
    {
        public event UnityAction<int> OnSoulsGained = delegate { };
        
        [SerializeField] int startingSouls;
        int _souls;
        public int Souls
        {
            get => _souls;
            set
            {
                _souls = value >= 0 ? value : 0;
                OnSoulsGained.Invoke(_souls);
            }
        }

        [SerializeField] Stats stats;
        public Stats Stats => stats;

        [SerializeField] float detectionRadius = 2;
        public float DetectionRadius => detectionRadius;

        [NonSerialized] public Transform Player;

        [field: NonSerialized] public bool UseSpawnPosition { get; private set; }
        [field: NonSerialized] public Vector3 SpawnPosition { get; private set; }

        // TODO: Class for stats
        

        public void Reset()
        {
            _souls = startingSouls;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            Reset();
        }

        public override object Save()
        {
            Vector3 position = Player.position;
            object statsData = stats.Save();
            return new SaveData(_souls, statsData, new []{position.x, position.y, position.z});
        }

        public override void Load(object data)
        {
            if (data is SaveData save)
            {
                _souls = save.souls;
                stats.Load(save.stats);
                SpawnPosition = new Vector3(save.position[0], save.position[1], save.position[2]);
                UseSpawnPosition = true;
                return;
            }

            Debug.LogError("Failed Loading");
        }

        [Serializable]
        struct SaveData
        {
            public int souls;
            public object stats;
            public float[] position;

            public SaveData(int souls, object stats, float[] position)
            {
                this.souls = souls;
                this.stats = stats;
                this.position = position;
            }
        }
    }
}
