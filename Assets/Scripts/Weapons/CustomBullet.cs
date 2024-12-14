//using Unity.FPS.Game;
//using UnityEngine;

///// <summary>
///// Represents a pistol bullet with specific properties and behaviours.
///// </summary>
//public class CustomBullet : ProjectileBase
//{
//    private void Start()
//    {
//        SetupPhysicsMaterial();
//    }

//    private void SetupPhysicsMaterial()
//    {
//        physicsMat = new PhysicMaterial
//        {
//            bounciness = bounciness,
//            frictionCombine = PhysicMaterialCombine.Minimum,
//            bounceCombine = PhysicMaterialCombine.Maximum
//        };

//        if (TryGetComponent(out SphereCollider sphereCollider))
//        {
//            sphereCollider.material = physicsMat;
//        }

//        rb.useGravity = useGravity;
//    }

//    protected override void Update()
//    {
//        base.Update();

//        if (collisions > maxCollisions)
//        {
//            Explode();
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        Debug.Log($"Collision with: {collision.gameObject.name}");

//        collisions++;

//        if (explodeOnTouch && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
//        {
//            Explode();
//        }
//    }

//    protected override void Explode()
//    {
//        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, explosionRange);

//        foreach (var obj in affectedObjects)
//        {
//            if (obj.TryGetComponent(out EnemyAI enemy))
//            {
//                enemy.TakeDamage(damage);
//                if (obj.attachedRigidbody != null)
//                {
//                    obj.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRange);
//                }
//            }
//        }

//        base.Explode();
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, explosionRange);
//    }
//}