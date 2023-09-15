using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace Player
{
    public class SpeechBubbleManager : MonoBehaviour
    {
        [Serializable]
        class Condition
        {
            [SerializeField] VariableStat.StatType stat;
            [SerializeField, Range(0,1)] float amount;
            [SerializeField, TextArea] string text;
            
            public VariableStat.StatType Stat => stat;
            public float Amount => amount;
            public string Text => text;

            public bool Triggered { get; set; }
        }
        
        [SerializeField] GameObject speechBubble;
        [SerializeField] Transform canvas;
        [SerializeField] List<Condition> conditions;
        [SerializeField] PlayerInfo info;

        [SerializeField] float minTimeBetweenBubbles = 10f;

        readonly Dictionary<VariableStat.StatType, SortedList<float, Condition>> _conditionsByStat = new Dictionary<VariableStat.StatType, SortedList<float, Condition>>();
        GameObject currentBubble;

        void Awake()
        {
            foreach (Condition condition in conditions)
            {
                if (_conditionsByStat.TryGetValue(condition.Stat, out SortedList<float, Condition> amounts))
                    amounts.Add(condition.Amount, condition);
                else
                    _conditionsByStat.Add(condition.Stat, new SortedList<float, Condition> {{condition.Amount, condition}});
            }
        }

        void OnEnable()
        {
            SubscribeEvents();
        }

        void OnDisable()
        {
            UnsubscribeEvents();
        }

        void SubscribeEvents()
        {
            foreach (VariableStat.StatType stat in _conditionsByStat.Keys)
            {
                info.Stats[stat].ValueChangedEvent += StatOnValueChangedEvent;
            }
        }
        
        void UnsubscribeEvents()
        {
            foreach (VariableStat.StatType stat in _conditionsByStat.Keys)
            {
                info.Stats[stat].ValueChangedEvent -= StatOnValueChangedEvent;
            }
        }

        void StatOnValueChangedEvent(VariableStat.StatType type, float amount)
        {
            // Only one bubble at a time
            if (currentBubble != null)
                return;
            
            float t = amount / info.Stats[type].MaxValue;
            foreach (Condition condition in _conditionsByStat[type].Values)
            {   
                if (condition.Amount < t)
                {
                    condition.Triggered = false;
                    continue;
                }
                
                if (condition.Triggered)
                    continue;
                
                currentBubble = Instantiate(speechBubble, canvas);
                currentBubble.GetComponentInChildren<TextMeshProUGUI>().text = condition.Text;
                condition.Triggered = true;
                
                StartCoroutine(Wait());
                
                return;
            }
        }

        IEnumerator Wait()
        {
            UnsubscribeEvents();
            
            yield return new WaitForSeconds(minTimeBetweenBubbles);
            
            SubscribeEvents();
        }
    }
}
