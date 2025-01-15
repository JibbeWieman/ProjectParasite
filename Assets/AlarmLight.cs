using UnityEngine;

public class AlarmLight : MonoBehaviour
{
    private Light alarmLight;

    void Start()
    {
        alarmLight = GetComponentInChildren<Light>();
        alarmLight.enabled = false;

        EventManager.AddListener<GameStartEvent>(TurnOnAlarm);
    }

    private void TurnOnAlarm(GameStartEvent evt)
    {
        alarmLight.enabled = true;
    }
}
