using System;
using Scriptable_Objects.Event_Channels;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Managers
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] OpenMenuChannel cookingMenuChannel;
        [SerializeField] Transform canvas;
        GameObject _menu;

        void OnEnable()
        {
            cookingMenuChannel.OnMenuOpen += OpenMenu;
            cookingMenuChannel.OnMenuClose += CloseMenu;
        }

        void CloseMenu()
        {
            _menu.SetActive(false);
        }

        void OpenMenu()
        {
            if (_menu == null)
                _menu = Instantiate(cookingMenuChannel.menu, canvas);

            _menu.SetActive(true);
        }
    }
}
