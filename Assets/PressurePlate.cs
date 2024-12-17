//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PressurePlate : MonoBehaviour
//{
//    private bool isPressurized;
//    [SerializeField]
//    private Animator animator;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.layer == LayerMask.NameToLayer("AI"))
//        {
//            isPressurized = true;
//            animator.SetInteger("TimesInteracted", +1);
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        isPressurized = false;
//        animator.SetInteger("TimesInteracted", -1);
//    }
//}
