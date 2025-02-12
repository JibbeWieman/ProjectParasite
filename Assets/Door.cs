using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public KeycardType requiredKey;

    protected Animator m_Animator;
    protected AudioSource m_AudioSource;
    [SerializeField]
    protected AudioClip doorOpenSfx, doorCloseSfx;

    protected GameObject passerby;
    private Coroutine closeDoorCoroutine;

    [SerializeField, Tooltip("The time it takes for the door to automatically close.")]
    private float closeDelay;

    public virtual void Start()
    {
        m_Animator = GetComponent<Animator>();
        DebugUtility.HandleErrorIfNullGetComponent<Animator, Door>(m_Animator, this, gameObject);

        m_AudioSource = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, Door>(m_AudioSource, this, gameObject);
    }

    protected void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("AI"))
        {
            var pass = collider.GetComponent<HighAuthorityPass>();
            if (pass != null && pass.keycardType == requiredKey)
            {
                passerby = collider.gameObject; // Set the passerby to the collider entered
                HandleDoorInteraction();
            }
        }
    }

    protected void OnTriggerExit(Collider collider)
    {
        if (passerby == collider.gameObject)
        {
            passerby = null;
            CloseDoor();
        }
    }

    public virtual void HandleDoorInteraction()
    {
        if (m_Animator == null || m_Animator.GetBool("OpenDoor")) return;

        m_Animator.SetBool("OpenDoor", true);
        PlaySound(doorOpenSfx);
        Debug.Log($"Opening door: {name}");

        // Cancel any ongoing close coroutine since the door is being opened
        if (closeDoorCoroutine != null)
        {
            StopCoroutine(closeDoorCoroutine);
            closeDoorCoroutine = null;
        }
    }

    protected void CloseDoor()
    {
        if (m_Animator == null || !m_Animator.GetBool("OpenDoor")) return;

        // Start coroutine only if not already running
        if (closeDoorCoroutine == null)
        {
            closeDoorCoroutine = StartCoroutine(CloseDoorRoutine());
        }
    }

    protected IEnumerator CloseDoorRoutine()
    {
        yield return new WaitForSeconds(closeDelay);

        if (m_Animator != null)
            m_Animator.SetBool("OpenDoor", false);

        PlaySound(doorCloseSfx);
        Debug.Log($"Closing door: {name}");

        closeDoorCoroutine = null; // Reset the coroutine reference
    }

    private void PlaySound(AudioClip clip)
    {
        if (m_AudioSource != null && clip != null)
        {
            m_AudioSource.PlayOneShot(clip);
        }
    }
}
