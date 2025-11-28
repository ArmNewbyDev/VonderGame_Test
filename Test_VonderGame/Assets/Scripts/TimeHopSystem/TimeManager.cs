using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    [SerializeField] private Volume volumeForDayTime;
    [SerializeField] private float TimeDurationForEachPeriod = 3f;
    [SerializeField] private List<GameObject> lightSources;
    public TimePriods currentTimePriod { get; private set; }
    public WeeklyCycle currentDay { get; private set; } = WeeklyCycle.Monday;
    public int DayTimeCounter { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        volumeForDayTime = GetComponent<Volume>();
        volumeForDayTime.weight = 0f;

    }



    public void TurnOnOrOffLightSource(bool isOn)
    {
        if (lightSources == null)
        {
            Debug.LogWarning("Light sources list is not assigned.");
            return;
        }

        foreach (var lightSource in lightSources)
        {
            lightSource.SetActive(isOn);
        }
    }


#region Day Cycle Management
    public void SetTimePriod(TimePriods timePriod)
    {
        currentTimePriod = timePriod;
        SetTimePriodsFunction();
    }


    private void SetTimePriodsFunction()
    {
        switch (currentTimePriod)
        {
            case TimePriods.Morning:
                StartCoroutine(SmoothChangeTime(0f,TimeDurationForEachPeriod));
                TurnOnOrOffLightSource(false);
                break;
            case TimePriods.Afternoon:
               StartCoroutine(SmoothChangeTime(0.15f,TimeDurationForEachPeriod));
                TurnOnOrOffLightSource(false);
                break;
            case TimePriods.Evening:
                StartCoroutine(SmoothChangeTime(0.5f,TimeDurationForEachPeriod));
                
                break;
            case TimePriods.Night:
                StartCoroutine(SmoothChangeTime(1f,TimeDurationForEachPeriod));
                
                break;
            default:
                Debug.LogWarning("Unhandled time period: " + currentTimePriod);
                break;
        }
    }

    IEnumerator SmoothChangeTime(float targetWeight, float durationTime = 3f)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;
        float tempVolumeForDayTime = volumeForDayTime.weight;

        while (elapsedTime < durationTime)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / durationTime; 
            volumeForDayTime.weight = Mathf.Lerp(tempVolumeForDayTime, targetWeight, t);

            yield return null; 
        }
        if (currentTimePriod == TimePriods.Night || currentTimePriod == TimePriods.Evening)
                TurnOnOrOffLightSource(true);

        volumeForDayTime.weight = targetWeight;
        Debug.Log("Transition Complete! Final Value: " + volumeForDayTime.weight);

    }
#endregion


#region Weekly Cycle Management
    public void GoToNextDay()
    {
        int currentDayIndex = (int)currentDay;
        int nextIndex = 0;
        int totalStates = Enum.GetValues(typeof(WeeklyCycle)).Length - 1; 


        if(currentDayIndex < totalStates)
        {
            nextIndex = currentDayIndex + 1;
        }
        else
        {
            nextIndex = 0;
        }
        currentDay = (WeeklyCycle)nextIndex;
        DayTimeCounter++;
        Debug.Log("DayTime Counter: " + DayTimeCounter.ToString());
        Debug.Log("Current Day is: " + currentDay.ToString());
        
    }


#endregion


}

[Serializable]
public enum TimePriods
{
    Morning, // wegiht = 0
    Afternoon, // wegiht = 0.15
    Evening, // wegiht = 0.5
    Night, // wegiht = 1
}

[Serializable]
public enum WeeklyCycle
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,

}