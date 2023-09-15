using System;
using System.Collections;
using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] SceneLoader playLevel;
        [SerializeField] SceneLoader randomLevel;
        [SerializeField] Button continueButton;

        [Header("Loading screen")] 
        [SerializeField] Image fillImage;
        
        void Awake()
        {
            if (!SaveManager.HasFile())
            {
                continueButton.gameObject.SetActive(false);
            }
        }
        
        public void Play(SceneLoader level)
        {
            StartCoroutine(LoadLevel(level));
        }


        public void Continue()
        {
            SaveManager.Load();
            
            StartCoroutine(LoadLevel(playLevel)); // TODO: Level to continue
        }

        IEnumerator LoadLevel(SceneLoader level)
        {
            StartCoroutine(level.LoadLevel());

            Time.timeScale = 0;
            
            while (level.Progress < 1f)
            {
                // Update progress bar
                fillImage.fillAmount = playLevel.Progress;
                yield return null;
            }
            
            AsyncOperation unloadScene = SceneManager.UnloadSceneAsync("MainMenu");

            Time.timeScale = 1;
            SaveManager.EnableSaving();
            
            // while (!unloadScene.isDone)
            // {
            //     yield return null;
            // }
        }

        public void Settings()
        {
            // TODO: for the volume and controls
        }

        public void Quit()
        {
            Application.Quit();
        }
  
    }
}
