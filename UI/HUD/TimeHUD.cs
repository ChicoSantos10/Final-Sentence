using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Scriptable_Objects;

public class TimeHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameData gameData;

    void Update()
    {
        text.text = gameData.GameTime.ToString();
    }
}
