using System;
using Player;
using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] GameObject gameOverMenu;

        [SerializeField] EventChannel gameOverChannel;
        [SerializeField] PlayerInfo player;
        [SerializeField] InputReader input;
        
        void OnEnable()
        {
            gameOverChannel.Event += GameOver;
            player.Stats[VariableStat.StatType.Hp].ValueChangedEvent += HpOnValueChangedEvent;
        }

        void OnDisable()
        {
            gameOverChannel.Event -= GameOver;
            player.Stats[VariableStat.StatType.Hp].ValueChangedEvent -= HpOnValueChangedEvent;
        }

        void GameOver()
        {
            // Disable input
            input.DisableGameplay();
            
            gameOverMenu.SetActive(true);
        }
        
        void HpOnValueChangedEvent(VariableStat.StatType type, float hp)
        {
            if (hp <= 0)
                GameOver();
        }
    }
}
