using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField] protected float m_JumpPadForce = 15f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.AddForce(transform.up * m_JumpPadForce, ForceMode.Impulse);
        }
    }
}