using UnityEngine;

public enum KeycardType
{
    None,
    Red,
    Green,
    Blue,
    Yellow
}

public class HighAuthorityPass : MonoBehaviour
{
    //private Animator animator;

    public bool hasInsertedKey = false;

    public KeyCode interactKey = KeyCode.E; // Customisable in Inspector

    private Collider currentTrigger;

    public KeycardType keycardType;


    private void OnTriggerEnter(Collider collider)
    {
        // Set the current trigger to the collider entered
        currentTrigger = collider;

        // Cache animator if it's a door
        //if (currentTrigger.CompareTag("Door"))
        //{
        //    animator = collider.GetComponent<Animator>();
        //    HandleDoorInteraction();
        //}
    }

    private void OnTriggerExit(Collider collider)
    {
        // Clear the trigger if leaving the collider
        if (currentTrigger == collider)
        {
            currentTrigger = null;
        }

        // Clear animator if leaving a door
        //if (collider.CompareTag("Door"))
        //{
        //    StartCoroutine(CloseDoor());
        //}
    }

    private void Update()
    {
        // No trigger to interact with
        if (currentTrigger == null) return;

        // Interact when the player presses the interact key
        if (Input.GetKeyDown(interactKey))
        {
            if (currentTrigger.CompareTag("KeyHole"))
            {
                HandleKeyHoleInteraction(currentTrigger.GetComponent<Keyhole>());
            }
        }
    }

    private void HandleKeyHoleInteraction(Keyhole keyhole)
    {
        if (keyhole == null) return;

        Debug.Log("Inserting key into keyhole...");
        keyhole.InsertKey();
    }

    //private void HandleDoorInteraction()
    //{
    //    if (animator == null || animator.GetBool("OpenDoor") || Elevator.isMoving) return;

    //    Debug.Log("Opening door...");
    //    animator.SetBool("OpenDoor", true);
    //    //StartCoroutine(CloseDoor());
    //}

    //private IEnumerator CloseDoor()
    //{
    //    yield return new WaitForSeconds(.5f);
    //    if (animator != null)
    //    {
    //        Debug.Log("Closing door...");
    //        animator.SetBool("OpenDoor", false);
    //    }
    //}
}
