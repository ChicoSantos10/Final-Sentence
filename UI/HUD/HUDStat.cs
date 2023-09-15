using Player;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class HUDStat : MonoBehaviour
    {
        [SerializeField] PlayerInfo stats;
        [SerializeField] Image image;
        [SerializeField] VariableStat.StatType statType;
        //[SerializeField] TextMeshProUGUI text;

        void Start()
        {
            UpdateValues(statType, stats.Stats[statType].CurrentValue);
        }

        private void OnEnable()
        {
            stats.Stats[statType].ValueChangedEvent += UpdateValues;
            UpdateValues(statType, stats.Stats[statType].CurrentValue);
        }

        private void UpdateValues(VariableStat.StatType type, float current)
        {
            VariableStat stat = stats.Stats[statType];
            //text.text = hp.ToString();
            image.fillAmount = current / stat.MaxValue;
        }

        private void OnDisable()
        {
            stats.Stats[statType].ValueChangedEvent -= UpdateValues;
        }
    }
}
