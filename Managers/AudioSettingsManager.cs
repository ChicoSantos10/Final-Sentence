using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Managers
{
    public class AudioSettingsManager : MonoBehaviour
    {
        [SerializeField] AudioMixer mixer;

        [SerializeField] Slider masterSlider, musicSlider, sfxSlider;
        [SerializeField] TextMeshProUGUI masterValue, musicValue, sfxValue;
        
        void OnEnable()
        {
            LoadSettings();
            AssignSliderValues();
            SetTextValues();
            
            SubscribeSliderEvents();
        }

        void OnDisable()
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
        }

        void LoadSettings()
        {
            mixer.SetFloat("Master_Volume", PlayerPrefs.GetFloat("Master_Volume"));
            mixer.SetFloat("Music_Volume", PlayerPrefs.GetFloat("Music_Volume"));
            mixer.SetFloat("SFX_Volume", PlayerPrefs.GetFloat("SFX_Volume"));
        }
        
        void AssignSliderValues()
        {
            if (mixer.GetFloat("Master_Volume", out float value))
                masterSlider.value = VolumeToValue(value);
            if (mixer.GetFloat("Music_Volume", out value))
                musicSlider.value = VolumeToValue(value);
            if (mixer.GetFloat("SFX_Volume", out value))
                sfxSlider.value = VolumeToValue(value);
        }

        void SetTextValues()
        {
            masterValue.text = ValueToText(masterSlider);
            musicValue.text = ValueToText(musicSlider);
            sfxValue.text = ValueToText(sfxSlider);
        }

        void SubscribeSliderEvents()
        {
            masterSlider.onValueChanged.AddListener(v =>
            {
                mixer.SetFloat("Master_Volume", ValueToVolume(v));
                PlayerPrefs.SetFloat("Master_Volume", ValueToVolume(v));
                masterValue.text = ValueToText(masterSlider);
            });
            musicSlider.onValueChanged.AddListener(v =>
            {
                mixer.SetFloat("Music_Volume", ValueToVolume(v));
                PlayerPrefs.SetFloat("Music_Volume", ValueToVolume(v));
                musicValue.text = ValueToText(musicSlider);
            });
            sfxSlider.onValueChanged.AddListener(v =>
            {
                mixer.SetFloat("SFX_Volume", ValueToVolume(v));
                PlayerPrefs.SetFloat("SFX_Volume", ValueToVolume(v));
                sfxValue.text = ValueToText(sfxSlider);
            });
        }

        static string ValueToText(Slider slider) => $"{slider.value:P}";

        /// <summary>
        /// Converts the db value to a number between 0 and 1
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        static float VolumeToValue(float volume) => Mathf.InverseLerp(-80, 20, volume);

        /// <summary>
        /// Converts a value between 0 and 1 to db values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static float ValueToVolume(float value) => Mathf.Lerp(-80, 20, value);
    }
}
