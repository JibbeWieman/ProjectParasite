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
        // Clear trigger when player leaves, but don't nullify animator if door coroutine is active
        if (currentTrigger == collider)
        {
            currentTrigger = null;
            if (collider.CompareTag("Door"))
            {
                // Ensure animator is cleared only if not mid-operation
                StartCoroutine(ClearAnimatorAfterCoroutine());
            }
        }
    }

    private IEnumerator ClearAnimatorAfterCoroutine()
    {
        yield return new WaitForSeconds(2); // Ensure CloseDoor has finished before nullifying
        animator = null;
    }

    private void Update()
    {
        // Check if player is in a trigger zone and presses the interact key
        if (currentTrigger != null)
        {
            if (currentTrigger.CompareTag("Door") && animator != null && !animator.GetBool("OpenDoor"))
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
        // Cache animator reference for coroutine safety
        Animator _Animator = animator;
        yield return new WaitForSeconds(2);

        // Close the door
        if (_Animator != null)
        {
            _Animator.SetBool("OpenDoor", false);
        }
    }
}
