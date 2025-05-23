using System.Collections.Generic;
using UnityEngine;

public class ParentPlatform : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        collision.transform.SetParent(null);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hit.transform.SetParent(transform);
    }
}
