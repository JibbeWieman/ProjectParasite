using UnityEngine;
using System.Collections.Generic;

public static class ConeCheck
{
    public static List<Collider> CheckCone(Vector3 origin, Vector3 direction, float angle, float range, LayerMask layerMask)
    {
        List<Collider> result = new List<Collider>();

        // Step 1: Perform a sphere check to get all potential targets within range
        Collider[] hits = Physics.OverlapSphere(origin, range, layerMask);

        // Step 2: Filter based on angle
        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = hit.transform.position - origin;
            float angleToTarget = Vector3.Angle(direction, directionToTarget);

            if (angleToTarget <= angle)
            {
                result.Add(hit);
            }
        }

        return result;
    }

    #region GIZMOS
    public static void DrawConeGizmo(Vector3 origin, Vector3 direction, float angle, float range)
    {
        // Draw the cone range as a line
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, range);

        // Draw the cone angle as a mesh or lines
        Gizmos.color = Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -angle, 0) * direction * range;
        Vector3 rightBoundary = Quaternion.Euler(0, angle, 0) * direction * range;

        Gizmos.DrawRay(origin, direction * range); // Forward line
        Gizmos.DrawRay(origin, leftBoundary); // Left boundary line
        Gizmos.DrawRay(origin, rightBoundary); // Right boundary line

        // Draw arc lines
        int segments = 20;
        float deltaAngle = angle * 2 / segments;
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = -angle + deltaAngle * i;
            float nextAngle = currentAngle + deltaAngle;

            Vector3 currentPoint = Quaternion.Euler(0, currentAngle, 0) * direction * range;
            Vector3 nextPoint = Quaternion.Euler(0, nextAngle, 0) * direction * range;

            Gizmos.DrawLine(origin + currentPoint, origin + nextPoint);
        }
    }
    #endregion
}
