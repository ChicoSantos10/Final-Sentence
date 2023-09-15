using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using TMPro;

public class DayHUD : MonoBehaviour
{
    [SerializeField] GameData date;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] EventChannel newDayEvent;

    void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = $"Day {date.Days}";
    }

    private void OnEnable()
    {
        newDayEvent.Event += UpdateText;
    }
    private void OnDisable()
    {
        newDayEvent.Event -= UpdateText;
    }
}
