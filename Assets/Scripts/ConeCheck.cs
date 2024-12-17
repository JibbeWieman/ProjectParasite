using UnityEngine;
using System.Collections.Generic;

public static class ConeCheck
{
    /// <summary>
    /// Checks for colliders within a cone-shaped area, with added height restrictions.
    /// </summary>
    public static List<Collider> CheckCone(Vector3 origin, Vector3 direction, float angle, float range, float height, LayerMask layerMask, bool checkRayCast = false)
    {
        List<Collider> result = new List<Collider>();

        // Step 1: Perform a sphere check to get all potential targets within range
        Collider[] hits = Physics.OverlapSphere(origin, range, layerMask);

        // Step 2: Filter based on angle and height using collider bounds
        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = (hit.bounds.center - origin).normalized;
            float distanceToTarget = Vector3.Distance(origin, hit.bounds.center);

            // Check if any part of the collider's bounds is within the height
            if (hit.bounds.max.y >= origin.y - height / 2f && hit.bounds.min.y <= origin.y + height / 2f)
            {
                // Check if the target is within the angle of the cone
                float angleToTarget = Vector3.Angle(direction, directionToTarget);
                if (angleToTarget <= angle)
                {
                    if (checkRayCast)
                    {
                        // Step 3: Ensure the target is within line of sight (no obstacles blocking)
                        if (!Physics.Raycast(origin, directionToTarget, distanceToTarget, ~layerMask))
                        {
                            result.Add(hit);
                        }
                    }
                    else
                    {
                        result.Add(hit);
                    }
                }
            }
        }

        return result;
    }

    #region GIZMOS
    /// <summary>
    /// Draws a cone gizmo in the scene view to visualise the range, angle, and height.
    /// </summary>
    public static void DrawConeGizmo(Vector3 origin, Vector3 direction, float angle, float range, float height)
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(origin, range);

        Gizmos.color = Color.red;

        // Draw base cone boundaries
        Vector3 leftBoundary = Quaternion.Euler(0, -angle, 0) * direction * range;
        Vector3 rightBoundary = Quaternion.Euler(0, angle, 0) * direction * range;

        Gizmos.DrawRay(origin, direction * range); // Forward line
        Gizmos.DrawRay(origin, leftBoundary); // Left boundary line
        Gizmos.DrawRay(origin, rightBoundary); // Right boundary line

        // Draw vertical height range
        Vector3 topOrigin = origin + Vector3.up * (height / 2f);
        Vector3 bottomOrigin = origin + Vector3.down * (height / 2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(topOrigin, bottomOrigin); // Height limits

        // Draw arcs for the top and bottom sections
        int segments = 20;
        float deltaAngle = angle * 2 / segments;

        for (int i = 0; i < segments; i++)
        {
            float currentAngle = -angle + deltaAngle * i;
            float nextAngle = currentAngle + deltaAngle;

            Vector3 currentTopPoint = topOrigin + Quaternion.Euler(0, currentAngle, 0) * direction * range;
            Vector3 nextTopPoint = topOrigin + Quaternion.Euler(0, nextAngle, 0) * direction * range;

            Vector3 currentBottomPoint = bottomOrigin + Quaternion.Euler(0, currentAngle, 0) * direction * range;
            Vector3 nextBottomPoint = bottomOrigin + Quaternion.Euler(0, nextAngle, 0) * direction * range;

            Gizmos.DrawLine(currentTopPoint, nextTopPoint);
            Gizmos.DrawLine(currentBottomPoint, nextBottomPoint);
        }
    }
    #endregion
}
