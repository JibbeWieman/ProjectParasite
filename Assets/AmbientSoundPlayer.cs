using System.Collections;
using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float minTime = 5f;  // Minimum time between sounds
    [SerializeField] private float maxTime = 15f; // Maximum time between sounds

    private AudioSource m_AudioSource;

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource == null)
        {
            Debug.LogError("AudioSource component missing on " + gameObject.name);
            return;
        }

        if (ambientSounds.Length == 0)
        {
            Debug.LogError("No ambient sounds assigned!");
            return;
        }

        StartCoroutine(PlayAmbientSounds());
    }

    private IEnumerator PlayAmbientSounds()
    {
        while (true)
        {
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            PlayRandomSound();
        }
    }

    private void PlayRandomSound()
    {
        if (ambientSounds.Length == 0) return;

        AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
        m_AudioSource.PlayOneShot(clip);
    }
}
