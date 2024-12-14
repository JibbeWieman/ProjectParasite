using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for all projectile behaviours, handles common projectile properties and shooting logic.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField]
    protected int baseDMG;
    [SerializeField]
    protected float bounciness;
    [SerializeField]
    protected float maxLifetime;
    [SerializeField]
    protected int maxCollisions;
    [Space(5)]
    [SerializeField]
    protected bool useGravity;
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

        OnShoot?.Invoke();
    }

    protected virtual void Start()
    {
        SetupPhysicsMaterial();

        rb = GetComponent<Rigidbody>();
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
                Destroy(gameObject);
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
        Destroy(gameObject);
    }

    private void SetupPhysicsMaterial()
    {
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