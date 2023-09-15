using System;
using UnityEngine;

namespace Player
{
    public class SpeechBubble : MonoBehaviour
    {
        [SerializeField, Tooltip("Time before disappearing")] float time;
        
        void Start()
        {
            Destroy(gameObject, time);
        }
    }
}
