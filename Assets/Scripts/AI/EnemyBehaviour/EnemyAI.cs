using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

public class EnemyAI : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    [SerializeField]
    private SceneTypeObject ST_Player;
    private GameObject player;
    private GameObject enemyTarget;
    public AudioSource enemyAudioHurt;
    private LayerMask TargetLayer;
    float m_LastTimeDamaged = float.NegativeInfinity;
    bool m_WasDamagedThisFrame;

    [Space(5)]

    public UnityAction onAttackTarget;
    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;
    public UnityAction onDamaged;

    private Actor m_Actor;
    private PatrolAgent m_PatrolAgent;
    private EnemyManager m_EnemyManager;
    private ActorsManager m_ActorsManager;
    private Health m_Health;
    private DetectionModule DetectionModule;
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
    public GameObject projectile;
    public GameObject pistolProjectile;
    public GameObject GunContainer;
    [HideInInspector] public bool friendlyFire = false;
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
        fleeing
    }
    #endregion

    private void Start()
    {
        player = ST_Player.Objects[0];

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
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);

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

        m_WasDamagedThisFrame = false;
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
            Debug.Log("Target detected: Chasing.");
            Chase();
        }
        else
        {
            Debug.Log("Target detected: Fleeing.");
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

    public void ReGroup(Vector3 position)
    {
        m_PatrolAgent.enabled = false;
        SetAgentDestination(position);
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
        var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
        Destroy(vfx, 5f);

        // tells the game flow manager to handle the enemy destuction
        m_EnemyManager.UnregisterEnemy(this);

        // this will call the OnDestroy function
        //Destroy(gameObject, DeathDuration);
        NavMeshAgent = null;
        m_PatrolAgent = null;
        DetectionModule = null;
    }

    //private void Update()
    //{
    //        if (DetectionModule.IsTargetInDetectionRange && DetectionModule.IsSeeingTarget)
    //        {
    //            if (!m_IsScared)
    //            {
    //                if (DetectionModule.IsTargetInAttackRange) Attack();
    //                else Chase();
    //            }
    //            else Flee();
    //        }
    //        else Patrol();
    //    }
    //}

    private void Update()
    {
        EnsureIsWithinLevelBounds();

        DetectionModule?.HandleTargetDetection(m_Actor, m_SelfColliders);

        enemyTarget = player;

        //Arm the ai with a projectile if it as a gun
        projectile = !IsStunned || !IsScared() ? pistolProjectile : null;

        if (!IsStunned && Events.ActorPossesedEvent.CurrentActor != GetComponent<Actor>().id)
        {
            if (DetectionModule.IsSeeingTarget)
            {
                if (DetectionModule.KnownDetectedTarget == player) 
                {
                    enemyTarget = player;
                }
                else if (DetectionModule.KnownDetectedTarget.layer == LayerMask.NameToLayer("AI") && 
                    DetectionModule.KnownDetectedTarget.GetComponent<ActorCharacterController>().IsDead &&
                    DetectionModule.KnownDetectedTarget.GetComponent<Actor>().IsActive())
                {
                    enemyTarget = DetectionModule.KnownDetectedTarget;
                }
            }

            switch (state)
            {
                case EnemyState.patrolling:
                    NavMeshAgent.enabled = false;
                    NavMeshAgent.speed = m_PatrolSpeed;
                    m_PatrolAgent.enabled = true;

                    Patrol();
                    break;

                case EnemyState.chasing:
                    NavMeshAgent.enabled = true;
                    NavMeshAgent.speed = m_ChaseSpeed;
                    m_PatrolAgent.enabled = false;

                    Chase();
                    break;

                case EnemyState.attacking:
                    NavMeshAgent.enabled = true;
                    NavMeshAgent.speed = m_AttackSpeed;
                    m_PatrolAgent.enabled = false;

                    Attack();
                    break;

                case EnemyState.fleeing:
                    NavMeshAgent.enabled = true;
                    NavMeshAgent.speed = m_FleeSpeed;
                    m_PatrolAgent.enabled = false;

                    Flee();
                    break;

                default:
                    // Optional: handle unexpected states
                    Debug.LogWarning($"Unhandled state: {state}");
                    break;
            }

            if (m_Health.CurrentHealth <= 0f)
            {
                NavMeshAgent.enabled = false;
                m_PatrolAgent.enabled = false;
            }
        }
    }

    #region STATE FUNCTIONS
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
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    public void AttackShooter(GameObject shooter)
    {
        if (shooter == null) return;

        // Set the enemytarget to the shooter
        enemyTarget = shooter;
        TargetLayer = LayerMask.GetMask("Host");
        StopAllCoroutines();
        StartCoroutine(SwitchTarget(shooter));
    }
    private IEnumerator SwitchTarget(GameObject shooter)
    {
        float elapsedTime = 0f;
        float focusTime = 10f; //Time to focus on the shooter

        // Continue checking the conditions for 5 seconds
        while (elapsedTime < focusTime)
        {
            if (shooter.IsDestroyed())
            {
                break;
            }

            // Wait for the next frame
            yield return null;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;
        }

        // After the focus time, switch back to the player
        enemyTarget = player;
        TargetLayer = LayerMask.GetMask("Player");
    }

    private void Flee()
    {
        if (state != EnemyState.fleeing)
            Debug.Log("Fleeing from player");

        state = EnemyState.fleeing;

        float distance = Vector3.Distance(transform.position, player.transform.position);

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
