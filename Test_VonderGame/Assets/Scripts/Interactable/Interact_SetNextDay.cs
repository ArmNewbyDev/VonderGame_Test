using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_SetNextDay : MonoBehaviour, IInteraction
{
    public void Interact()
    {
        TimeManager.Instance.GoToNextDay();
        
    }

}
