using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreechAbility : AbilityBase
{
    #region VARIABLES
    [Header("References")]
    private Transform parasiteBody;                               // The position to spawn the screech particles
    private GameObject instantiatedObject;                        // Reference to the instantiated object

    public GameObject screechPrefab;                              // The prefab to spawn for the ability effect
    [Range(1f, 3f)] public float particleLifetime;                // The lifetime of the particles

    public float stunDuration = 2f;                               // Duration for which enemies will be stunned
    public float screechRange = 16f;                               // Range within which enemies will be stunned
    public float coneAngle = 16f;                                 // Angle of the cone in degrees

    private LayerMask collisionLayer;                              // Layers to detect enemies
    [SerializeField] private KeyCode key = KeyCode.Mouse3;        // Keycode that activates the ability

    #endregion

    private void Start()
    {
        parasiteBody = GetComponentInChildren<Animator>().gameObject.transform;
        collisionLayer = LayerMask.GetMask("Host");
    }

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            TriggerAbility();
        }

        // Continuously update the prefab's rotation
        if (instantiatedObject != null && parasiteBody != null)
        {
            instantiatedObject.transform.rotation = parasiteBody.rotation;
        }
    }

    public override void Ability()
    {
        StartCoroutine(AbilityCoroutine());
    }
    private IEnumerator AbilityCoroutine()
    {
        // Instantiate and set up the object
        instantiatedObject = Instantiate(screechPrefab, transform.position, parasiteBody.rotation);
        instantiatedObject.transform.SetParent(gameObject.transform);

        Destroy(instantiatedObject, particleLifetime);

        // Add a small delay before checking the cone
        yield return new WaitForSeconds(0.5f);

        // Detect and stun enemies within cone range
        List<Collider> hitColliders = ConeCheck.CheckCone(transform.position, parasiteBody.forward, coneAngle, screechRange, collisionLayer);

        foreach (var hitCollider in hitColliders)
        {
            EnemyAI enemy = hitCollider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                Debug.Log($"Stunning enemy: {enemy.name}");
                enemy.Stun(stunDuration);
            }
        }
    }

    #region GIZMOS
    private void OnDrawGizmosSelected()
    {
        // Draw a wire cone in the Scene view for visualizing the range
        ConeCheck.DrawConeGizmo(transform.position, transform.forward, coneAngle, screechRange);
    }
    #endregion
}