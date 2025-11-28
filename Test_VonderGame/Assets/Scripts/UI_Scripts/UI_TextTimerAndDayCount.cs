using TMPro;
using UnityEngine;

public class UI_TextTimerAndDayCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DayText;
    [SerializeField] private TextMeshProUGUI DayCountText;


    void Update()
    {
        DayCountText.text = "Day: " + TimeManager.Instance.DayTimeCounter.ToString();
        DayText.text = TimeManager.Instance.currentDay.ToString();
    }

}
