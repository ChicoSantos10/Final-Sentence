using System;
using Audio;
using Scriptable_Objects;
using UnityEngine;

namespace Enemies
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] GameData data;
        [SerializeField] GameObject boss;

        [Header("Music")]
        [SerializeField] AudioChannelSO channel;
        [SerializeField] AudioClip bossMusic;
        
        void Start()
        {
            string bossName = boss.name;
            
            if (data.DefeatedBosses.Contains(bossName))
                return;

            Instantiate(boss, transform.position, Quaternion.identity).name = bossName;
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            print("Boss Encounter");
            channel.PlayAudio(bossMusic);
        }
    }
}
