using System;
using Scriptable_Objects;
using UnityEngine;

namespace Player
{
    public class Hazard : MonoBehaviour
    {
        [SerializeField] int damage;
        [SerializeField] float timePerTick = 0.3f;
        [SerializeField] PlayerInfo info;
        [SerializeField] VariableStat.StatType stat;

        float _timeInside;

        void OnTriggerEnter2D(Collider2D other)
        {
            info.Stats[stat].Reduce(damage);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (_timeInside < timePerTick)
            {
                _timeInside += Time.deltaTime;
                
                return;
            }
            
            info.Stats[stat].Reduce(damage);

            _timeInside = 0;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            _timeInside = 0;
        }
    }
}
