using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace AI.BehaviourTree.Module
{
    public class Blackboard
    {
        readonly Dictionary<string, object> _blackboard = new Dictionary<string, object>();
        readonly Dictionary<string, UnityAction> _valueChangedEvents = new Dictionary<string, UnityAction>();
        readonly Dictionary<string, UnityAction> _valueSetEvents = new Dictionary<string, UnityAction>();

        public bool TryGetValue<T>(string id, out T value)
        {
            value = default;
            
            if (_blackboard.TryGetValue(id, out object obj))
            {
                if (obj is T data)
                {
                    value = data;
                    return true;
                }

                Debug.LogError($"Tried to cast data of type: {obj.GetType()} to: {typeof(T)}");
            }
            
            return false;
        }

        public void SetValue(string id, object value)
        {
            if (_blackboard.TryGetValue(id, out object _))
            {
                if (!_blackboard[id].Equals(value) && _valueChangedEvents.TryGetValue(id, out UnityAction a))
                {
                    a.Invoke();
                }
                
                _blackboard[id] = value;
                if (_valueSetEvents.TryGetValue(id, out a))
                    a.Invoke();
            }
            else
                _blackboard.Add(id, value);
        }

        /// <summary>
        /// Sets a value without invoking any events
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetValueNoEvents(string id, object value)
        {
            if (_blackboard.TryGetValue(id, out object _))
            {
                _blackboard[id] = value;
            }
            else
            {
                _blackboard.Add(id, value);
            }
        }

        public void SubscribeEvent(string id, UnityAction a, EventType evtType)
        {
            switch (evtType)
            {
                case EventType.Set:
                    Subscribe(id, a, _valueSetEvents);
                    break;
                case EventType.ValueChanged:
                    Subscribe(id, a, _valueChangedEvents);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evtType), evtType, null);
            }
        }

        void Subscribe(string id, UnityAction a, IDictionary<string, UnityAction> events)
        {
            if (events.TryGetValue(id, out _))
                events[id] += a;
            else
                events.Add(id, a);
        }
        
        public void UnsubscribeEvent(string id, UnityAction a, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Set:
                    Unsubscribe(id, a, _valueSetEvents);
                    break;
                case EventType.ValueChanged:
                    Unsubscribe(id, a, _valueChangedEvents);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
            
        }

        void Unsubscribe(string id, UnityAction a, IDictionary<string, UnityAction> events)
        {
            if (events.TryGetValue(id, out _))
                events[id] += a;
        }
    }
}
