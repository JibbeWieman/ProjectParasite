using UnityEngine;

/// <summary>
/// Represents a pistol bullet with specific properties and behaviours.
/// </summary>
public class PistolProjectile : ProjectileBase
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}