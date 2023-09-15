using Player;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class HungerHUD : MonoBehaviour
    {

        [SerializeField] PlayerInfo stats;
        [SerializeField] Image image;
        //[SerializeField] TextMeshProUGUI text;

        void Start()
        {
            UpdateValues(VariableStat.StatType.Hunger, stats.Stats[VariableStat.StatType.Hunger].CurrentValue);
        }

        private void OnEnable()
        {
            stats.Stats[VariableStat.StatType.Hunger].ValueChangedEvent += UpdateValues;
            UpdateValues(VariableStat.StatType.Hunger, stats.Stats[VariableStat.StatType.Hunger].CurrentValue);
        }

        private void UpdateValues(VariableStat.StatType type, float current)
        {
            VariableStat hunger = stats.Stats[VariableStat.StatType.Hunger];
            //text.text = hunger.ToString();
            image.fillAmount = current / hunger.MaxValue;
        }

        private void OnDisable()
        {
            stats.Stats[VariableStat.StatType.Hunger].ValueChangedEvent -= UpdateValues;
        }
    }
}
