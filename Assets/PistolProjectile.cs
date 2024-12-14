using UnityEngine;

/// <summary>
/// Represents a pistol bullet with specific properties and behaviours.
/// </summary>
public class PistolProjectile : ProjectileBase
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision with: {collision.gameObject.name}");

        collisions++;

        if (explodeOnTouch)
        {
            Explode();
        }
    }

    protected override void Explode()
    {
        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, explosionRange);

        foreach (var obj in affectedObjects)
        {
            if (obj.TryGetComponent(out EnemyAI enemy))
            {
                enemy.TakeDamage(explosionDMG);
                if (obj.attachedRigidbody != null)
                {
                    obj.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRange);
                }
            }
        }

        base.Explode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}