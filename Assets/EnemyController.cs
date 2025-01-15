using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using static EnemyAI;

[RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer Renderer;
        public int MaterialIndex;

        public RendererIndexData(Renderer renderer, int index)
        {
            Renderer = renderer;
            MaterialIndex = index;
        }
    }

    [Header("Parameters")]
    [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
    public float SelfDestructYHeight = -20f;

    [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
    public float PathReachingRadius = 2f;

    [Tooltip("The speed at which the enemy rotates")]
    public float OrientationSpeed = 10f;

    [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
    public float DeathDuration = 0f;

    [Space(5)]
    [SerializeField]
    private DetectionModule DetectionModule;
    public NavMeshAgent NavMeshAgent { get; private set; }


    [Header("Weapons Parameters")]
    [Tooltip("Allow weapon swapping for this enemy")]
    public bool SwapToNextWeapon = false;

    [Tooltip("Time delay between a weapon swap and the next attack")]
    public float DelayAfterWeaponSwap = 0f;

    [Header("Flash on hit")]
    [Tooltip("The material used for the body of the hoverbot")]
    public Material BodyMaterial;

    [Tooltip("The gradient representing the color of the flash on hit")]
    [GradientUsageAttribute(true)]
    public Gradient OnHitBodyGradient;

    [Tooltip("The duration of the flash on hit")]
    public float FlashOnHitDuration = 0.5f;

    [Header("Sounds")]
    [Tooltip("Sound played when recieving damages")]
    public AudioClip DamageTick;

    [Header("VFX")]
    [Tooltip("The VFX prefab spawned when the enemy dies")]
    public GameObject DeathVfx;

    [Tooltip("The point at which the death VFX is spawned")]
    public Transform DeathVfxSpawnPoint;

    [Header("Debug Display")]
    [Tooltip("Color of the sphere gizmo representing the path reaching range")]
    public Color PathReachingRangeColor = Color.yellow;

    [Tooltip("Color of the sphere gizmo representing the attack range")]
    public Color AttackRangeColor = Color.red;

    [Tooltip("Color of the sphere gizmo representing the detection range")]
    public Color DetectionRangeColor = Color.blue;

    public UnityAction onAttack;
    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;
    public UnityAction onDamaged;

    List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
    MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
    float m_LastTimeDamaged = float.NegativeInfinity;

    [SerializeField]
    private PatrolAgent m_PatrolAgent;
    public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
    public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
    public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
    public bool HadKnownTarget => DetectionModule.HadKnownTarget;

    int m_PathDestinationNodeIndex;
    EnemyManager m_EnemyManager;
    ActorsManager m_ActorsManager;
    Health m_Health;
    Actor m_Actor;
    Collider[] m_SelfColliders;
    GameFlowManager m_GameFlowManager;
    bool m_WasDamagedThisFrame;
    float m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;
    int m_CurrentWeaponIndex;
    WeaponController m_CurrentWeapon;
    WeaponController[] m_Weapons;
    NavigationModule m_NavigationModule;

    void Start()
    {
        m_EnemyManager = FindObjectOfType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);

        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);

        //m_EnemyManager.RegisterEnemy(this);

        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);

        m_Actor = GetComponent<Actor>();
        DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);

        NavMeshAgent = GetComponent<NavMeshAgent>();
        m_PatrolAgent = GetComponent<PatrolAgent>();
        m_SelfColliders = GetComponentsInChildren<Collider>();

        m_GameFlowManager = FindObjectOfType<GameFlowManager>();
        DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(m_GameFlowManager, this);

        // Subscribe to damage & death actions
        m_Health.OnDie += OnDie;
        m_Health.OnDamaged += OnDamaged;

        // Find and initialize all weapons
        FindAndInitializeAllWeapons();
        var weapon = GetCurrentWeapon();
        weapon.ShowWeapon(true);

        var detectionModules = GetComponentsInChildren<DetectionModule>();
        DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
            gameObject);
        DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
            this, gameObject);
        // Initialize detection module
        DetectionModule = detectionModules[0];
        DetectionModule.onDetectedTarget += OnDetectedTarget;
        DetectionModule.onLostTarget += OnLostTarget;
        onAttack += DetectionModule.OnAttack;

        var navigationModules = GetComponentsInChildren<NavigationModule>();
        DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
            this, gameObject);
        // Override navmesh agent data
        if (navigationModules.Length > 0)
        {
            m_NavigationModule = navigationModules[0];
            NavMeshAgent.speed = m_NavigationModule.MoveSpeed;
            NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
            NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == BodyMaterial)
                {
                    m_BodyRenderers.Add(new RendererIndexData(renderer, i));
                }
            }
        }

        m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        onDetectedTarget += HandleDetectedTarget;
        onLostTarget += HandleLostTarget;
    }

    private void OnDisable()
    {
        onDetectedTarget -= HandleDetectedTarget;
        onLostTarget -= HandleLostTarget;
    }

    void Update()
    {
        EnsureIsWithinLevelBounds();

        DetectionModule?.HandleTargetDetection(m_Actor, m_SelfColliders);

        Color currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
        m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
        foreach (var data in m_BodyRenderers)
        {
            data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
        }

        m_WasDamagedThisFrame = false;
    }

    void EnsureIsWithinLevelBounds()
    {
        // at every frame, this tests for conditions to kill the enemy
        if (transform.position.y < SelfDestructYHeight)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnLostTarget()
    {
        onLostTarget.Invoke();
    }
    void HandleLostTarget()
    {
        if (Events.ActorPossesedEvent.CurrentActor != m_Actor.id)
        {
            m_PatrolAgent.enabled = true;
        }
    }

    //private void Flee()
    //{
    //    m_NavigationModule.speed = m_FleeSpeed;

    //    agent.enabled = true;
    //    waypointMover.enabled = false;
    //    rb.isKinematic = false;

    //    float distance = Vector3.Distance(transform.position, player.transform.position);

    //    if (distance < enemyDistanceFlee)
    //    {
    //        //Vector player to me
    //        Vector3 dirToPlayer = transform.position - enemyTarget.transform.position;

    //        Vector3 newPos = transform.position + dirToPlayer;

    //        SetAgentDestination(newPos);
    //    }
    //}

    void OnDetectedTarget()
    {
        onDetectedTarget.Invoke();
    }
    void HandleDetectedTarget()
    {
        if (m_CurrentWeapon != null)
        {
            OrientTowards(m_ActorsManager.Player.transform.position);
            m_PatrolAgent.enabled = false;
        }
        else
        {
            Flee();
        }
    }

    //void Attack()
    //{
    //    Debug.Log("Attacking player");

    //    //Make sure enemy doesn't move
    //    SetNavDestination(transform.position);

    //    transform.LookAt(enemyTarget.transform);

    //    if (!alreadyAttacked)
    //    {
    //        ///Attack code here
    //        Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
    //        rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
    //        rb.AddForce(transform.up * 8f, ForceMode.Impulse);

    //        alreadyAttacked = true;
    //        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    //    }
    //}

    //private void ResetAttack()
    //{
    //    alreadyAttacked = false;
    //}

    public void OrientTowards(Vector3 lookPosition)
    {
        Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
        if (lookDirection.sqrMagnitude != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
        }
    }
    public void ResetPathDestination()
    {
        m_PathDestinationNodeIndex = 0;
    }

    private void Flee()
    {
        m_PatrolAgent.enabled = false;

        //float distance = Vector3.Distance(transform.position, m_ActorsManager.Player.transform.position);

        //if (distance < enemyDistanceFlee)
        //{
        //Vector player to me
        Vector3 dirToPlayer = transform.position - m_ActorsManager.Player.transform.position;

        Vector3 newPos = transform.position + dirToPlayer;

        SetNavDestination(newPos);
        //}
    }
    private void SetNavDestination(Vector3 destination)
    {
        if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
        {
            NavMeshAgent.SetDestination(destination);
        }
    }

    void OnDamaged(float damage, GameObject damageSource)
    {
        // test if the damage source is the player
        if (damageSource && !damageSource.GetComponent<EnemyController>())
        {
            // pursue the player
            DetectionModule.OnDamaged(damageSource);

            onDamaged?.Invoke();
            m_LastTimeDamaged = Time.time;

            // play the damage tick sound
            if (DamageTick && !m_WasDamagedThisFrame)
                AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);

            m_WasDamagedThisFrame = true;
        }
    }

    void OnDie()
    {
        // spawn a particle system when dying
        var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
        Destroy(vfx, 5f);

        // tells the game flow manager to handle the enemy destuction
        //m_EnemyManager.UnregisterEnemy(this);

        // this will call the OnDestroy function
        //Destroy(gameObject, DeathDuration);
        NavMeshAgent = null;
        m_PatrolAgent = null;
        DetectionModule = null;
    }

    void OnDrawGizmosSelected()
    {
        // Path reaching range
        Gizmos.color = PathReachingRangeColor;
        Gizmos.DrawWireSphere(transform.position, PathReachingRadius);

        if (DetectionModule != null)
        {
            // Detection range
            Gizmos.color = DetectionRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);

            // Attack range
            Gizmos.color = AttackRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
        }
    }

    public void OrientWeaponsTowards(Vector3 lookPosition)
    {
        for (int i = 0; i < m_Weapons.Length; i++)
        {
            // orient weapon towards player
            Vector3 weaponForward = (lookPosition - m_Weapons[i].WeaponRoot.transform.position).normalized;
            m_Weapons[i].transform.forward = weaponForward;
        }
    }

    public bool TryAtack(Vector3 enemyPosition)
    {
        if (m_GameFlowManager.GameIsEnding)
            return false;

        OrientWeaponsTowards(enemyPosition);

        if ((m_LastTimeWeaponSwapped + DelayAfterWeaponSwap) >= Time.time)
            return false;

        // Shoot the weapon
        bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);

        if (didFire && onAttack != null)
        {
            onAttack.Invoke();

            if (SwapToNextWeapon && m_Weapons.Length > 1)
            {
                int nextWeaponIndex = (m_CurrentWeaponIndex + 1) % m_Weapons.Length;
                SetCurrentWeapon(nextWeaponIndex);
            }
        }

        return didFire;
    }

    void FindAndInitializeAllWeapons()
    {
        // Check if we already found and initialized the weapons
        if (m_Weapons == null)
        {
            m_Weapons = GetComponentsInChildren<WeaponController>();
            DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(m_Weapons.Length, this,
                gameObject);

            for (int i = 0; i < m_Weapons.Length; i++)
            {
                m_Weapons[i].Owner = gameObject;
            }
        }
    }

    public WeaponController GetCurrentWeapon()
    {
        FindAndInitializeAllWeapons();
        // Check if no weapon is currently selected
        if (m_CurrentWeapon == null)
        {
            // Set the first weapon of the weapons list as the current weapon
            SetCurrentWeapon(0);
        }

        DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(m_CurrentWeapon, this,
            gameObject);

        return m_CurrentWeapon;
    }

    void SetCurrentWeapon(int index)
    {
        m_CurrentWeaponIndex = index;
        m_CurrentWeapon = m_Weapons[m_CurrentWeaponIndex];
        if (SwapToNextWeapon)
        {
            m_LastTimeWeaponSwapped = Time.time;
        }
        else
        {
            m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;
        }
    }
}