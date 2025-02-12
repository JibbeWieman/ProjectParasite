using UnityEngine;

public class AlarmLight : MonoBehaviour
{
    [SerializeField] private Material lightOn_mat, lightOff_mat;
    private Renderer m_Renderer;
    private AudioSource m_AudioSource;

    private Light alarmLight;

    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        m_AudioSource = GetComponent<AudioSource>();
        alarmLight = GetComponentInChildren<Light>();

        EventManager.AddListener<GameStartEvent>(TurnOnAlarm);
        EventManager.AddListener<ElevatorUnlockedEvent>(TurnOffAlarm);
    }

    private void TurnOnAlarm(GameStartEvent evt)
    {
        alarmLight.enabled = true;
        m_AudioSource.enabled = true;

        Material[] mats = m_Renderer.materials; // Get a copy of the materials array
        mats[1] = lightOn_mat; // Change the second material
        m_Renderer.materials = mats; // Assign the modified array back
    }

    private void TurnOffAlarm(ElevatorUnlockedEvent evt)
    {
        alarmLight.enabled = false;
        m_AudioSource.enabled = false;

        Material[] mats = m_Renderer.materials; // Get a copy of the materials array
        mats[1] = lightOff_mat; // Change the second material
        m_Renderer.materials = mats; // Assign the modified array back
    }

}
