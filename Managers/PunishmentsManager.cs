using System;
using System.Collections.Generic;
using Enemies;
using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using Scriptable_Objects.Punishments;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PunishmentsManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] PlayerInfo playerInfo;
        [SerializeField] GameData data;
        
        [Header("Event channels")]
        [SerializeField] EventChannel newDayEvent;
        [SerializeField] OnBossKilledChannel bossKilledEvent;
        [SerializeField] EventChannel gameOverChannel;

        [SerializeField] List<Punishment> availablePunishments;
        [SerializeField] int punishmentProbability;
        
        [SerializeField] NumberVariable soulsMultiplier;

        public int NextSacrifice => Mathf.Max((int) Mathf.Log(data.Days - 1), 0); // Max not necessary probably

        void OnEnable()
        {
            newDayEvent.Event += OnNewDay;
            bossKilledEvent.Event += OnBossKilledEvent;
        }
        
        void OnDisable()
        {
            newDayEvent.Event -= OnNewDay;      
            bossKilledEvent.Event -= OnBossKilledEvent;
        }

        void TributeSouls()
        {
            int soulsAfterSacrifice = playerInfo.Souls - NextSacrifice * (int)soulsMultiplier;
            
            if (soulsAfterSacrifice < 0)
                GameOver();
            else
                playerInfo.Souls = soulsAfterSacrifice;
        }

        void OnNewDay()
        {
            // Check if punishment should be applied
            if (Random.Range(0, 100) < punishmentProbability)
            {
                availablePunishments[Random.Range(0, availablePunishments.Count)].Begin();
            }
            
            TributeSouls();
        }
        
        void OnBossKilledEvent(Boss boss)
        {
            availablePunishments.Remove(boss.Punishment);
            data.DefeatedBosses.Add(boss.name);
        }

        void GameOver()
        {
            gameOverChannel.Invoke();
        }
    }
}
