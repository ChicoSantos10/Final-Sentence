using System;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class DayCycleManager : MonoBehaviour
    {
        [SerializeField] GameData data;
        [SerializeField] AnimationCurve lightCurve;
        [SerializeField] Light2D globalLight; // Needs to find it instead of serialized??
        
        [SerializeField, Tooltip("How many in-game minutes per real second")] float ratio = 1.2F;

        void Update()
        {
            if (data.GameTime.AddTime(Time.deltaTime * ratio))
                data.Days++;
            
            //data.GameTime.SetTime((int) _hours, (int) _minutes);
            
            //print($"Time: {_hours}:{_minutes}");

            globalLight.intensity = lightCurve.Evaluate((data.GameTime.Hours * 60 + data.GameTime.Minutes) / 720);
        }
    }
}
