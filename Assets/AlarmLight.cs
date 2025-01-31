using UnityEngine;

public class AlarmLight : MonoBehaviour
{
    private Renderer m_Renderer;
    [SerializeField] private Material lightOn_mat, lightOff_mat;

    private Light alarmLight;

    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        alarmLight = GetComponentInChildren<Light>();

        EventManager.AddListener<GameStartEvent>(TurnOnAlarm);
        EventManager.AddListener<ElevatorUnlockedEvent>(TurnOffAlarm);
    }

    private void TurnOnAlarm(GameStartEvent evt)
    {
        alarmLight.enabled = true;

        Material[] mats = m_Renderer.materials; // Get a copy of the materials array
        mats[1] = lightOn_mat; // Change the second material
        m_Renderer.materials = mats; // Assign the modified array back
    }

    private void TurnOffAlarm(ElevatorUnlockedEvent evt)
    {
        alarmLight.enabled = false;

        Material[] mats = m_Renderer.materials; // Get a copy of the materials array
        mats[1] = lightOff_mat; // Change the second material
        m_Renderer.materials = mats; // Assign the modified array back
    }

}
