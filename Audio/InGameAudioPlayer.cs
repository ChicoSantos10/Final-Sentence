using System;
using Enemies;
using Scriptable_Objects.Event_Channels;
using UnityEngine;

namespace Audio
{
    public class InGameAudioPlayer : MonoBehaviour
    {
        [SerializeField] AudioChannelSO channel;
        [SerializeField] AudioClip inGameTheme;
        [SerializeField] OnBossKilledChannel bossKilledChannel;
        
        void Start()
        {
            channel.PlayAudio(inGameTheme);
        }

        void OnEnable()
        {
            bossKilledChannel.Event += OnBossKilled;
        }
        
        void OnDisable()
        {
            bossKilledChannel.Event -= OnBossKilled;
        }

        void OnBossKilled(Boss _)
        {
            channel.PlayAudio(inGameTheme);
        }
    }
}
