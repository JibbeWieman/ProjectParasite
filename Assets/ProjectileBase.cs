using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.Rendering.UI;

/// <summary>
/// Base class for all projectile behaviours, handles common projectile properties and shooting logic.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField, Tooltip("The base amount of damage of the bullet")]
    protected int baseDMG;
    [SerializeField, Tooltip("How much the bullet bounces against objects")]
    protected float bounciness;
    [SerializeField, Tooltip("The maximum amount of time before the bullet gets destroyed")]
    protected float maxLifetime;
    [SerializeField, Tooltip("The maximum amount of collisions before the bullet gets destroyed")]
    protected int maxCollisions;
    [Space(5)]
    [SerializeField, Tooltip("Does the bullet get affected by gravity")]
    protected bool useGravity;
    [Tooltip("If the bullet explodes upon contact or end of lifetime")]
    public bool explodes;

    [SerializeField] protected GameObject explosion;
    [SerializeField] protected int explosionDMG;
    [SerializeField] protected float explosionRange;
    [SerializeField] protected float explosionForce;
    [Space(5)]
    [SerializeField] protected bool explodeOnTouch;

    protected int collisions;
    protected PhysicMaterial physicsMat;
    protected Rigidbody rb;

    public GameObject Owner { get; private set; }
    public Vector3 InitialPosition { get; private set; }
    public Vector3 InitialDirection { get; private set; }
    public Vector3 InheritedMuzzleVelocity { get; private set; }
    public float InitialCharge { get; private set; }

    public UnityAction OnShoot;

    /// <summary>
    /// Initializes the projectile when fired.
    /// </summary>
    public void Shoot(WeaponController controller)
    {
        Owner = controller.Owner;
        InitialPosition = transform.position;
        InitialDirection = transform.forward;
        InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
        InitialCharge = controller.CurrentCharge;
        
        if (!enabled)
            enabled = true;

        OnShoot?.Invoke();

        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb)
        //{
        //    Vector3 shotDirection;
        //    if (Owner.GetComponent<Actor>().IsActive())
        //    {
        //        shotDirection = controller.GetShotDirection();
        //    }
        //    else
        //    {
        //        shotDirection = controller.GetAIShotDirection(controller.GetComponent<EnemyAI>().enemyTarget);
        //    }
        //    rb.AddForce(shotDirection * 40f, ForceMode.VelocityChange);
        //}

        if (!controller.UsesPooling)
        {
            Destroy(gameObject, 5f);
        }
    }

    protected virtual void Start()
    {
        SetupPhysicsMaterial();
    }

    /// <summary>
    /// Handles the countdown for projectile lifetime.
    /// </summary>
    protected virtual void Update()
    {
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0 || collisions > maxCollisions)
        {
            if (explodes)
            {
                Explode();
            }
            else
            {
                Deactivate(gameObject);  //'Destroy' object and add it back to the pool
            }
        }
    }


    /// <summary>
    /// Triggers the explosion behaviour of the projectile.
    /// </summary>
    protected virtual void Explode()
    {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, explosionRange);

        foreach (var obj in affectedObjects)
        {
            if (obj.TryGetComponent(out Health enemy))
            {
                enemy.TakeDamage(explosionDMG, this.transform.root.gameObject);
                if (obj.attachedRigidbody != null)
                {
                    obj.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRange);
                }
            }
        }

        Deactivate(gameObject);  //'Destroy' object and add it back to the pool
    }

    private void SetupPhysicsMaterial()
    {
        rb = GetComponent<Rigidbody>();

        physicsMat = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        if (TryGetComponent(out SphereCollider sphereCollider))
        {
            sphereCollider.material = physicsMat;
        }

        rb.useGravity = useGravity;
    }

    protected void Deactivate(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"Collision with: {collision.gameObject.name}");

        if (collision.gameObject == Owner) return;
        else if (collision.gameObject.TryGetComponent(out Health hitActor))
        {
            if (hitActor != null)
                hitActor.TakeDamage(baseDMG, transform.root.gameObject);
        }

        collisions++;

        if (explodeOnTouch)
        {
            Explode();
        }
    }

}

#region Custom Inspector
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(ProjectileBase), true)] // 'true' enables derived classes to use this editor
public class ProjectileBaseEditor : Editor
{
    private SerializedProperty explodesProp;
    private SerializedProperty explosionProp;
    private SerializedProperty explosionRangeProp;
    private SerializedProperty explosionForceProp;
    private SerializedProperty explosionDMGProp;
    private SerializedProperty explodeOnTouchProp;

    private void OnEnable()
    {
        // Cache the serialized properties for use in OnInspectorGUI
        explodesProp = serializedObject.FindProperty("explodes");
        explosionProp = serializedObject.FindProperty("explosion");
        explosionRangeProp = serializedObject.FindProperty("explosionRange");
        explosionForceProp = serializedObject.FindProperty("explosionForce");
        explosionDMGProp = serializedObject.FindProperty("explosionDMG");
        explodeOnTouchProp = serializedObject.FindProperty("explodeOnTouch");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw individual fields explicitly
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseDMG"), new GUIContent("Base Damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bounciness"), new GUIContent("Bounciness"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLifetime"), new GUIContent("Max Lifetime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxCollisions"), new GUIContent("Max Collisions"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useGravity"), new GUIContent("Use Gravity"));
        EditorGUILayout.PropertyField(explodesProp, new GUIContent("Explodes"));

        // Conditionally draw explosion settings
        if (explodesProp.boolValue)
        {
            EditorGUILayout.Space(10); // Adds 10 pixels of spacing
            EditorGUILayout.LabelField("Explosion Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(explosionProp, new GUIContent("Explosion Prefab"));
            EditorGUILayout.PropertyField(explosionRangeProp, new GUIContent("Explosion Range"));
            EditorGUILayout.PropertyField(explosionForceProp, new GUIContent("Explosion Force"));
            EditorGUILayout.PropertyField(explosionDMGProp, new GUIContent("Explosion Damage"));
            EditorGUILayout.PropertyField(explodeOnTouchProp, new GUIContent("Explode On Touch"));
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }

}
#endif


#endregion