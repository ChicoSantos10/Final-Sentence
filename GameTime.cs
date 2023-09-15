using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameTime : ISerializationCallbackReceiver
{
    [SerializeField] int startingHours, startingMinutes;
    
    float _minutes;
    float _hours;

    public float Minutes => _minutes;

    public float Hours => _hours;

    public void SetTime(float hours, float minutes)
    {
        _hours = hours;
        _minutes = minutes;
    }

    /// <summary>
    /// Adds time
    /// </summary>
    /// <param name="minutes">The minutes to add</param>
    /// <returns>If its a new day</returns>
    public bool AddTime(float minutes)
    {
        _minutes += minutes;

        if (_minutes < 60) 
            return false;

        _minutes = 0;
        _hours++;

        if (_hours < 24) 
            return false;
        
        _hours = 0;
        return true;
    }

    public override string ToString()
    {
        return $"{_hours:00}:{_minutes:00}";
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        _minutes = startingMinutes;
        _hours = startingHours;
    }
}
