using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public KeycardType requiredKey;

    protected Animator m_Animator;

    protected GameObject passerby;

    public virtual void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    protected void OnTriggerEnter(Collider collider)
    {
        passerby = collider.gameObject;                 // Set the passerby to the collider entered

        if (passerby.layer == LayerMask.NameToLayer("AI") &&
            passerby.GetComponent<HighAuthorityPass>().keycardType == requiredKey)
        {
            HandleDoorInteraction();
        }
    }

    protected void OnTriggerExit(Collider collider)
    {
        if (passerby == collider.gameObject)           // Clear the trigger if leaving the collider
        {
            passerby = null;
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("AI"))
        {
            StartCoroutine(CloseDoor());
        }
    }

    public virtual void HandleDoorInteraction()
    {
        if (m_Animator == null || m_Animator.GetBool("OpenDoor")) return;
        
        m_Animator.SetBool("OpenDoor", true);
        Debug.Log("Opening door...");
    }

    protected IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(1f);
        
        m_Animator.SetBool("OpenDoor", false);
        Debug.Log("Closing door...");
    }
}