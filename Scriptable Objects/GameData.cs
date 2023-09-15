using System;
using System.Collections.Generic;
using Enemies;
using Managers;
using SaveData;
using Scriptable_Objects.Event_Channels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = "Data/" + nameof(GameData))]
    public class GameData : SaveableScriptableObject
    {
        [SerializeField] EventChannel newDayEvent;

        [NonSerialized] int _days = 1;
        public int Days
        {
            get => _days;
            set
            {
                _days = value;
                
                if (_days > 1)
                    newDayEvent.Invoke();
            }
        }

        [SerializeField] GameTime gameTime;
        public GameTime GameTime => gameTime;

        [field: NonSerialized] public List<string> DefeatedBosses { get; private set; } = new List<string>();

        public override object Save()
        {
            return new SaveData(_days, GameTime.Minutes, GameTime.Hours, DefeatedBosses);
        }

        public override void Load(object data)
        {
            if (!(data is SaveData saveData))
            {
                Debug.LogError("Failed loading");
                return;
            }
            
            _days = saveData.days;
            GameTime.SetTime(saveData.hours, saveData.minutes);
            DefeatedBosses = saveData.bossNames;
        }

        [Serializable]
        struct SaveData
        {
            public int days;
            public float minutes, hours;
            public List<string> bossNames;

            public SaveData(int days, float minutes, float hours, List<string> bossNames)
            {
                this.days = days;
                this.minutes = minutes;
                this.hours = hours;
                this.bossNames = bossNames;
            }
        }
    }
}
