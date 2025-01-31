using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Events;
using Unity.Services.Analytics.Internal;

public class EnemyAI : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    [SerializeField]
    private SceneTypeObject ST_Player;
    private Animator m_Animator;
    private GameObject player;
    public GameObject enemyTarget { get; private set; }
    public AudioSource enemyAudioHurt;
    private LayerMask TargetLayer;
    float m_LastTimeDamaged = float.NegativeInfinity;
    bool m_WasDamagedThisFrame;

    [Space(5)]

    public UnityAction onAttackTarget;
    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;
    public UnityAction onDamaged;

    public Actor m_Actor;
    private EnemyManager m_EnemyManager;
    private ActorWeaponsManager m_ActorWeaponsManager;
    private ActorsManager m_ActorsManager;
    private Health m_Health;
    private DetectionModule DetectionModule;
    public PatrolAgent m_PatrolAgent { get; private set; }
    public NavMeshAgent NavMeshAgent { get; private set; }

    [Header("Variables")]
    [SerializeField] private float m_PatrolSpeed;
    [SerializeField] private float m_ChaseSpeed;
    [SerializeField] private float m_AttackSpeed;
    [SerializeField] private float m_FleeSpeed;

    private bool IsStunned;

    // Properties to access them if needed elsewhere
    [HideInInspector] public float PatrolSpeed { get => m_PatrolSpeed; private set => m_PatrolSpeed = value; }
    [HideInInspector] public float ChaseSpeed { get => m_ChaseSpeed; private set => m_ChaseSpeed = value; }
    [HideInInspector] public float AttackSpeed { get => m_AttackSpeed; private set => m_AttackSpeed = value; }
    [HideInInspector] public float FleeSpeed { get => m_FleeSpeed; private set => m_FleeSpeed = value; }

    [Header("Attacking")]
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    //public GameObject projectile;
    [SerializeField] private GameObject pistolProjectile;
    //public GameObject GunContainer;
    //[HideInInspector] public bool friendlyFire = false;
    Collider[] m_SelfColliders;

    [Header("States")]
    public EnemyState state;

    [Header("Parameters")]
    [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
    public float SelfDestructYHeight = -20f;

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

    public enum EnemyState
    {
        patrolling,
        chasing,
        attacking,
        fleeing,
        regrouping,
    }
    #endregion

    private void Start()
    {
        player = ST_Player.Objects[0];
        enemyTarget = player;

        m_Animator = GetComponentInChildren<Animator>();

        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);

        m_Actor = GetComponent<Actor>();
        DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);

        NavMeshAgent = GetComponent<NavMeshAgent>();
        m_PatrolAgent = GetComponent<PatrolAgent>();
        m_SelfColliders = GetComponentsInChildren<Collider>();

        TargetLayer = LayerMask.GetMask("Player");

        m_EnemyManager = FindObjectOfType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);

        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyAI, EnemyController>(m_ActorsManager, this);

        m_ActorWeaponsManager = GetComponent<ActorWeaponsManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyAI, ActorWeaponsManager>(m_ActorWeaponsManager, this);


        m_EnemyManager.RegisterEnemy(this);

        var detectionModules = GetComponentsInChildren<DetectionModule>();
        DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
            gameObject);
        DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
            this, gameObject);
        // Initialize detection module
        DetectionModule = detectionModules[0];
        DetectionModule.onDetectedTarget += OnDetectedTarget;
        DetectionModule.onLostTarget += OnLostTarget;
        onAttackTarget += DetectionModule.OnAttack;
        m_Health.OnDie += OnDie;

        m_WasDamagedThisFrame = false;

        EventManager.AddListener<OnBodyFoundEvent>(Regroup); 
    }

    private void OnEnable()
    {
        onDetectedTarget += HandleDetectedTarget;
        onAttackTarget -= Attack;
        onLostTarget += Patrol;
    }
    private void OnDisable()
    {
        onDetectedTarget -= HandleDetectedTarget;
        onAttackTarget -= Attack;
        onLostTarget -= Patrol;
    }

    void OnDetectedTarget()
    {
        onDetectedTarget.Invoke();
    }
    void OnAttackTarget()
    {
        onAttackTarget.Invoke();
    }
    void OnLostTarget()
    {
        onLostTarget.Invoke();
    }

    private void HandleDetectedTarget()
    {
        if (!IsScared())
        {
            Chase();
        }
        else
        {
            Flee();
        }
    }
    private bool IsScared()
    {
        if (GetComponentInChildren<ActorWeaponsManager>().GetActiveWeapon() == false || m_Health.CurrentHealth <= 30) //.ActiveWeaponIndex != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Regroup(OnBodyFoundEvent evt)
    {
        state = EnemyState.regrouping;

        SetAgentDestination(evt.Body.transform.position);

        if (transform.position == evt.Body.transform.position)
        {
            state = EnemyState.patrolling;
        }
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
        GameObject vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
        Destroy(vfx, 5f);

        // tells the game flow manager to handle the enemy destuction
        m_EnemyManager.UnregisterEnemy(this);

        // this will call the OnDestroy function
        //Destroy(gameObject, DeathDuration);
        NavMeshAgent = null;
        m_PatrolAgent = null;
        DetectionModule = null;
    }

    private void Update()
    {
        EnsureIsWithinLevelBounds();

        DetectionModule?.HandleTargetDetection(m_Actor, m_SelfColliders);
        if (m_Health.CurrentHealth <= 0f)
        {
            SetAgentState(0f, false, false);
            return;
        }

        //Arm the ai with a projectile if it as a gun
        //projectile = !IsStunned || !IsScared() ? pistolProjectile : null;

        if (!IsStunned && !GetComponent<Actor>().IsActive())
        {
            if (DetectionModule.IsSeeingTarget)
            {
                HandleTargetDetection();
            }

            StateHandler();
        }
    }
    private void HandleTargetDetection()
    {
        if (DetectionModule.KnownDetectedTarget.CompareTag("Host") &&
            DetectionModule.KnownDetectedTarget.GetComponentInParent<ActorCharacterController>().IsDead &&
            DetectionModule.KnownDetectedTarget.GetComponentInParent<Actor>().IsActive())
        {
            enemyTarget = DetectionModule.KnownDetectedTarget;
        }
        else if (DetectionModule.KnownDetectedTarget.transform.root.CompareTag("Player"))
        {
            enemyTarget = player;
        }
        else if (!DetectionModule.KnownDetectedTarget.CompareTag("Host") &&
            !DetectionModule.KnownDetectedTarget.GetComponentInParent<ActorCharacterController>().IsDead &&
            !DetectionModule.KnownDetectedTarget.GetComponentInParent<Actor>().IsActive() &&
            !DetectionModule.KnownDetectedTarget.transform.root.CompareTag("Player"))
        {
            enemyTarget = null;
            OnLostTarget();
        }

        //Debug.Log($"Enemy target set to: {enemyTarget}");
    }

    #region STATE FUNCTIONS
    private void StateHandler()
{
    switch (state)
    {
        case EnemyState.patrolling:
            SetAgentState(m_PatrolSpeed, true, true);
            Patrol();
            break;
        case EnemyState.chasing:
            SetAgentState(m_ChaseSpeed, true, false);
            Chase();
            break;
        case EnemyState.attacking:
            SetAgentState(m_AttackSpeed, true, false);
            Attack();
            break;
        case EnemyState.fleeing:
            SetAgentState(m_FleeSpeed, true, false);
            Flee();
            break;
        case EnemyState.regrouping:
            SetAgentState(m_PatrolSpeed, true, false);
            Regroup(Events.OnBodyFoundEvent);
            break;
        default:
            Debug.LogWarning($"Unhandled state: {state}");
            break;
    }
}
    private void SetAgentState(float speed, bool navEnabled, bool patrolEnabled)
    {
        NavMeshAgent.enabled = navEnabled;
        NavMeshAgent.speed = speed;
        m_PatrolAgent.enabled = patrolEnabled;
    }

    private void Patrol()
    {
        if (state != EnemyState.patrolling)
            Debug.Log("Patrolling.");

        state = EnemyState.patrolling;
    }

    private void Chase()
    {
        if (state != EnemyState.chasing)
            Debug.Log("Chasing Player");

        state = EnemyState.chasing;

        float distanceToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);

        if (distanceToTarget <= DetectionModule.AttackRange)
        {
            state = EnemyState.attacking;
            return;
        }

        SetAgentDestination(enemyTarget.transform.position);
    }

    private void Attack()
    {
        if (state != EnemyState.attacking)
            Debug.Log("Attacking player");

        state = EnemyState.attacking;

        //Make sure enemy doesn't move
        SetAgentDestination(transform.position);
        transform.LookAt(enemyTarget.transform);

        if (!alreadyAttacked)
        {
            WeaponController weapon = m_ActorWeaponsManager.GetActiveWeapon();

            weapon.AIShoot(enemyTarget);
            Debug.Log(enemyTarget);

            alreadyAttacked = true;
        }
        Invoke(nameof(ResetAttack), timeBetweenAttacks);

        /* ///Attack code here
        //Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        //rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
        //rb.AddForce(transform.up * 8f, ForceMode.Impulse);
        //} */
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    //public void AttackShooter(GameObject shooter)
    //{
    //    if (shooter == null) return;

    //    // Set the enemytarget to the shooter
    //    enemyTarget = shooter;
    //    TargetLayer = LayerMask.GetMask("Host");
    //    StopAllCoroutines();
    //    StartCoroutine(SwitchTarget(shooter));
    //}
    //private IEnumerator SwitchTarget(GameObject shooter, float focusTime = 10f)
    //{
    //    float elapsedTime = 0f;

    //    while (elapsedTime < focusTime && shooter != null && !shooter.IsDestroyed())
    //    {
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    // Reset to player
    //    enemyTarget = player;
    //    TargetLayer = LayerMask.GetMask("Player");
    //}

    private void Flee()
    {
        if (state != EnemyState.fleeing)
            Debug.Log("Fleeing from player");

        state = EnemyState.fleeing;

        float distance = Vector3.Distance(transform.position, enemyTarget.transform.position);

        if (distance < DetectionModule.DetectionRange)
        {
            Vector3 dirToPlayer = transform.position - enemyTarget.transform.position;          //Vector player to me

            Vector3 newPos = transform.position + dirToPlayer;

            SetAgentDestination(newPos);
        }
    }
    private void SetAgentDestination(Vector3 destination)
    {
        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
        {
            NavMeshAgent.SetDestination(destination);
        }
    }
    #endregion

    /*
    public void Stun(float stunDuration)
    {
        Debug.Log(this.name + "is stunned for" + stunDuration + " seconds");
        if (!m_IsStunned)
        {
            m_IsStunned = true;

            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            m_PatrolAgent.enabled = false;

            StartCoroutine(EndStunAfterDuration(stunDuration));
        }
    }
    private IEnumerator EndStunAfterDuration(float stunDuration)
    {
        yield return new WaitForSeconds(stunDuration);
        m_IsStunned = false;

        NavMeshAgent.enabled = true;
        m_PatrolAgent.enabled = true;
    }

    private void OnParticleCollision(GameObject other)
    {
        Stun(5f);
    } */

    #region GIZMOS
    void OnDrawGizmosSelected()
    {
        // Path reaching range
        Gizmos.color = PathReachingRangeColor;

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
    #endregion
}
