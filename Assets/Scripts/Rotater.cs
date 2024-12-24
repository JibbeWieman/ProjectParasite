using UnityEngine;

/// <summary>
/// Rotates a gameObject around the specified axis at the specified speed
/// </summary>
public class Rotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 45;
    [SerializeField] private Vector3 rotationAxis;
    [SerializeField] private float rotationRangeDegrees = 90; // Maximum angle the object will rotate in both directions

    /// <summary>
    /// Do the rotation
    /// </summary>
    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Draws gizmos to visualise the rotation range when the object is selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        float radius = 1f;

        Gizmos.color = Color.yellow;

        // Compute the forward direction based on the rotation axis
        Vector3 forward = transform.forward.normalized;
        Vector3 right = Vector3.Cross(rotationAxis.normalized, forward).normalized * radius;
        Vector3 up = Vector3.Cross(forward, right).normalized * radius;

        // Draw the circle to represent the rotation plane
        for (int i = 0; i <= 360; i += 10)
        {
            float angleRad1 = Mathf.Deg2Rad * i;
            float angleRad2 = Mathf.Deg2Rad * (i + 10);
            Vector3 point1 = transform.position + right * Mathf.Cos(angleRad1) + up * Mathf.Sin(angleRad1);
            Vector3 point2 = transform.position + right * Mathf.Cos(angleRad2) + up * Mathf.Sin(angleRad2);
            Gizmos.DrawLine(point1, point2);
        }

        // Draw lines indicating the rotation range
        Vector3 startDirection = Quaternion.AngleAxis(-rotationRangeDegrees / 2, rotationAxis) * forward;
        Vector3 endDirection = Quaternion.AngleAxis(rotationRangeDegrees / 2, rotationAxis) * forward;
        Gizmos.DrawLine(transform.position, transform.position + startDirection * radius);
        Gizmos.DrawLine(transform.position, transform.position + endDirection * radius);
    }
}
