using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighAuthorityPass : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    private bool hasInteractedWithElevator = false;

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the colliding object has the tag "Player"
        if (collider.CompareTag("Door"))
        {
            // Trigger the animation
            animator = collider.GetComponent<Animator>();
            animator.SetBool("OpenDoor", true);
            StartCoroutine(CloseDoor());
        }

        // Check if the colliding object has the tag "Player"
        if (collider.CompareTag("ElevatorDoor"))
        {
            // Trigger the animation
            animator = collider.GetComponent<Animator>();
            if (!hasInteractedWithElevator)
            {
                animator.SetInteger("TimesInteracted",+1);
            }
            if (animator.GetInteger("TimesInteracted") >= 2)
            {
                animator.SetBool("OpenDoor", true);
            }

            StartCoroutine(CloseDoor());
        }
    }


    private IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(2);

        animator.SetBool("OpenDoor", false);
    }
}