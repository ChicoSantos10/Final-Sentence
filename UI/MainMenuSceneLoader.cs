using System;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuSceneLoader : MonoBehaviour
    {
        void Awake()
        {
            SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        }
    }
}
