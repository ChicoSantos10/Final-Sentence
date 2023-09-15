using System;
using Managers;
using Player;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] Text soulsText;
        
        [SerializeField] PlayerInfo info;

        [SerializeField] SceneLoader restartLevel;

        void OnEnable()
        {
            // soulsText.text = info.Souls + " SOULS"; // TODO: Currently no way to tell how many total souls were collected, Add later
            // TODO: Maybe explain the reasoning for losing (e.g. No souls for sacrifice, lost all hp)

            //Time.timeScale = 0;
        }

        void OnDisable()
        {
            Time.timeScale = 1;
        }

        public void Restart()
        {
            // TODO: Fix
            restartLevel.LoadLevel();
        }

        public void Quit()
        {
            Application.Quit();
        }

    }
}
