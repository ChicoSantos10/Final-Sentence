using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class Stats : ISerializationCallbackReceiver
    {
        public VariableStat this[VariableStat.StatType type] => _variableStats[type];
        public Stat this[Stat.StatType type] => _stats[type]; 
        
        [SerializeField] VariableStat[] variableStats;
        Dictionary<VariableStat.StatType, VariableStat> _variableStats;
        
        [SerializeField] Stat[] stats;
        Dictionary<Stat.StatType, Stat> _stats;

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            _variableStats = new Dictionary<VariableStat.StatType, VariableStat>();
            _stats = new Dictionary<Stat.StatType, Stat>();
            
            // Variable stats
            foreach (VariableStat stat in variableStats)
            {
                if (_variableStats.TryGetValue(stat.Type, out VariableStat s))
                    Debug.LogWarning($"Stat already present: {s.Type}. Will be ignored");
                else
                    _variableStats.Add(stat.Type, stat);
            }
            
            // Fixed stats
            foreach (Stat stat in stats)
            {
                if (_stats.TryGetValue(stat.Type, out Stat s))
                    Debug.LogWarning($"Stat already present: {s.Type}. Will be ignored");
                else
                    _stats.Add(stat.Type, stat);
            }
            
            foreach (VariableStat stat in _variableStats.Values)
            {
                stat.Reset();
            }
        }

        public object Save()
        {
            Dictionary<Stat.StatType, object> statsData = _stats.Values.ToDictionary(stat => stat.Type, stat => stat.Save());
            Dictionary<VariableStat.StatType, object> variableStatsData = _variableStats.Values.ToDictionary(stat => stat.Type, stat => stat.Save());
            
            return new SaveData(statsData, variableStatsData);
        }

        public void Load(object data)
        {
            if (data is SaveData save)
            {
                foreach (KeyValuePair<Stat.StatType, object> saveData in save.Stats)
                {
                    _stats[saveData.Key].Load(saveData.Value);
                }
                
                foreach (KeyValuePair<VariableStat.StatType, object> saveData in save.VariableStats)
                {
                    _variableStats[saveData.Key].Load(saveData.Value);
                }
            }
            else
            {
                Debug.LogError("Load failed");
            }
        }

        [Serializable]
        struct SaveData
        {
            public Dictionary<Stat.StatType, object> Stats;
            public Dictionary<VariableStat.StatType, object> VariableStats;

            public SaveData(Dictionary<Stat.StatType, object> stats, Dictionary<VariableStat.StatType, object> variableStats)
            {
                Stats = stats;
                VariableStats = variableStats;
            }
        }
    }
}
