using System;
using Scriptable_Objects;
using UnityEngine;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject pauseMenu;
        [SerializeField] InputReader input;

        // Start is called before the first frame update
        void Start()
        {
            pauseMenu.SetActive(false);
        }

        void OnEnable()
        {
            input.OnPauseAction += OnPause;
        }

        void OnDisable()
        {
            input.OnPauseAction -= OnPause;
        }

        /// <summary>
        /// Switches between paused and resumed state
        /// </summary>
        public void OnPause()
        {
            bool isPaused = pauseMenu.activeSelf;
            pauseMenu.SetActive(!isPaused);
            Time.timeScale = Convert.ToSingle(isPaused); 
        }

        public void GoToMainMenu()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close the Game
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
        }

        // TODO: SAVE GAME
    }
}
