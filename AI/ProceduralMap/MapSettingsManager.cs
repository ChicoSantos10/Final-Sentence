using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AI.ProceduralMap
{
    public class MapSettingsManager : MonoBehaviour
    {
        [SerializeField] MapSettings settings;
        
        [Header("Seed")]
        [SerializeField] TMP_InputField input;

        [Header("Map Size")]
        [SerializeField] Button mapSizeButton;
        [SerializeField] Transform mapSizeButtonsParent;
        Outline _currentSelectedOutline;
        
        void Awake()
        {
            SetInputStartText();
            CreateMapSizeButtons();
        }

        void OnEnable()
        {
            input.onValidateInput += OnValidateInput;
            input.onSubmit.AddListener(Submit);
        }
        
        void OnDisable()
        {
            input.onValidateInput -= OnValidateInput;
            input.onSubmit.RemoveListener(Submit);
        }

        #region Seed

        void SetInputStartText()
        {
            input.characterLimit = MapSettings.MaxDigitString;
            input.text = settings.Seed.ToString();
        }
        
        static char OnValidateInput(string text, int charIndex, char newChar)
        {
            return int.TryParse(newChar.ToString(), out int _) ? newChar : '\0';
        }

        void Submit(string s)
        {
            if (s.Equals(""))
            {
                input.text = settings.Seed.ToString();
                return;
            }

            settings.Seed = int.Parse(s);
        }

        #endregion

        #region Map Size

        void CreateMapSizeButtons()
        {
            foreach (MapSize size in Enum.GetValues(typeof(MapSize)))
            {
                Button b = Instantiate(mapSizeButton, mapSizeButtonsParent);
                Outline outline = b.GetComponent<Outline>();
                TextMeshProUGUI text = b.GetComponentInChildren<TextMeshProUGUI>();
                text.text = size.ToString();

                if (settings.MapSize == size)
                {
                    _currentSelectedOutline = outline;
                    outline.enabled = true;
                }
                
                b.onClick.AddListener(() =>
                {
                    _currentSelectedOutline.enabled = false;
                    outline.enabled = true;
                    settings.MapSize = size;
                    _currentSelectedOutline = outline;
                });
            }
        }

        #endregion
    }
}
