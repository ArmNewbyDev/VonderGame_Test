using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_SetDayTime : MonoBehaviour, IInteraction
{
    public TimePriods timePriodToSet = TimePriods.Morning;


    public void Interact()
    {
        TimeManager.Instance.SetTimePriod(timePriodToSet);
        Debug.Log("Time period set to: " + timePriodToSet.ToString());
    }

}
