using System.Collections;
using UnityEngine;

public class HighAuthorityPass : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    // Track if the key has been inserted
    public bool hasInsertedKey = false;

    // Player input key for interaction
    public KeyCode interactKey = KeyCode.E; // Customisable in Inspector

    // Trigger area detection
    private Collider currentTrigger;

    private ElevatorDoor doorToUnlock; 



    private void OnTriggerEnter(Collider collider)
    {
        // Store the trigger to use in Update
        if (collider.CompareTag("Door"))
        {
            currentTrigger = collider;
            animator = collider.GetComponent<Animator>();
        }

        if (collider.CompareTag("KeyHole"))
        {
            currentTrigger = collider;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Clear trigger when player leaves
        if (currentTrigger == collider)
        {
            currentTrigger = null;
            animator = null;
        }
    }

    private void Update()
    {
        // Check if player is in a trigger zone and presses the interact key
        if (currentTrigger != null)
        {
            if (currentTrigger.CompareTag("Door") && animator.GetBool("OpenDoor") == false && animator != null)
            {
                HandleDoorInteraction();
            }
            else if (currentTrigger.CompareTag("KeyHole") && Input.GetKeyDown(interactKey))
            {
                InsertKey();
            }
        }
    }

    private void Start()
    {
        doorToUnlock = FindAnyObjectByType<ElevatorDoor>();
    }

    private void HandleDoorInteraction()
    {
        // Trigger the door animation
        animator.SetBool("OpenDoor", true);
        StartCoroutine(CloseDoor());
    }

    private void InsertKey()
    {
        if (!hasInsertedKey)
        {
            Debug.Log("Inserting key");
            // Mark key as inserted
            hasInsertedKey = true;

            doorToUnlock.UpdateKeyAmount(1);
        }
    }

    private IEnumerator CloseDoor()
    {
        Animator m_Animator = animator;
        yield return new WaitForSeconds(2);

        // Close the door
        if (m_Animator != null)
        {
            animator.SetBool("OpenDoor", false);
        }
    }
}
