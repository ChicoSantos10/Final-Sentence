using Player;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class StaminaHUD : MonoBehaviour
    {
        [SerializeField] PlayerInfo stats;
        [SerializeField] Image image;
        //[SerializeField] TextMeshProUGUI text;

        void Start()
        {
            UpdateValues(VariableStat.StatType.Stamina, stats.Stats[VariableStat.StatType.Stamina].CurrentValue);
        }

        private void OnEnable()
        {
            stats.Stats[VariableStat.StatType.Stamina].ValueChangedEvent += UpdateValues;
            UpdateValues(VariableStat.StatType.Stamina, stats.Stats[VariableStat.StatType.Stamina].CurrentValue);
        }

        private void UpdateValues(VariableStat.StatType type, float current)
        {
            VariableStat stamina = stats.Stats[VariableStat.StatType.Stamina];
            //text.text = stamina.ToString();
            image.fillAmount = current / stamina.MaxValue;
        }

        private void OnDisable()
        {
            stats.Stats[VariableStat.StatType.Stamina].ValueChangedEvent -= UpdateValues;
        }
    }
}
