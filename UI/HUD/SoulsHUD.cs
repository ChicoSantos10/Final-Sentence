using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    public class SoulsHUD : MonoBehaviour
    {
        [SerializeField] PlayerInfo souls;
        [SerializeField] TextMeshProUGUI text;

        void Start()
        {
            UpdateText(souls.Souls);
        }

        // Update is called once per frame
        void UpdateText(int souls)
        {
            text.text = $"{souls}";
        }

        private void OnEnable()
        {
            souls.OnSoulsGained += UpdateText;
        }
        private void OnDisable()
        {
            souls.OnSoulsGained -= UpdateText;
        }
    }
}
