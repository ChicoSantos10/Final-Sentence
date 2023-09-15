using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace Enemies
{
    public class Pentagram : MonoBehaviour
    {
        [SerializeField] GameObject lucifer;

        [SerializeField] AudioChannelSO channel;
        [SerializeField] AudioClip finalBossMusic;
        
        void Start()
        {
            SummonLucifer();
        }

        void SummonLucifer()
        {
            channel.PlayAudio(finalBossMusic);
            Instantiate(lucifer, transform.position, Quaternion.identity);
        }
    }
}
