using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TriggerPlatform : MonoBehaviour
{
    MovingPlatform platform;
    Animator animator;

    //[SerializeField] float delayTime = 5f; // Time delay before the platform moves again

    private void Start()
    {
        platform = GetComponent<MovingPlatform>();
        animator = GetComponentInChildren<Animator>();
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    // Check if the specified animation has finished
    //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 is the layer index
    //    if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f)
    //    {
    //        platform.canMove = true;
    //        //StopCoroutine(StartMovementAfterDelay()); // Stop any existing coroutine
    //    }
    //}

    public void ElevatorUp()
    {
        //Check if the specified animation has finished
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 is the layer index
        if (stateInfo.IsName("Metal_Door_Close") && stateInfo.normalizedTime >= 1.0f)
        {
            platform.canMove = true;
        }
    }

    public void ElevatorDown()
    {
        platform.canMove = true;
    }

    /* public IEnumerator StartMovementAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        platform.canMove = true;
    } */
}
