using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class EnemyAI : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    public GameObject player;
    public GameObject enemyTarget;
    public Rigidbody rb;
    public NavMeshAgent agent;
    public GameObject floatingText;
    public AudioSource enemyAudioHurt;
    public LayerMask whatIsGround, whatIsTarget;
    [Space(5)]
    [SerializeField] private WaypointMover waypointMover;

    [Header("Variables")]
    public float m_MaxHealth;
    public float m_CurrentHealth;
    public bool m_IsDead = false;
    public bool m_IsScared; //If the AI isn't armed they get scared and try to flee
    private bool m_IsStunned;
    
    private float m_Speed;
    [Space(10)]
    [SerializeField] private float m_PatrolSpeed;
    [SerializeField] private float m_ChaseSpeed;
    [SerializeField] private float m_AttackSpeed;
    [SerializeField] private float m_FleeSpeed;

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

    [Header("Fleeing")]
    private float enemyDistanceFlee;

    [Header("States")]
    public EnemyState state;
    [Space(5)]
    public float sightRange;
    public float attackRange;
    public float fleeRange;
    [Space(5)]
    public bool targetVisible;
    public bool targetInSightRange, targetInAttackRange, targetInFleeRange;

    /*[Header("Vignette Effect")]
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField] private float vignetteRedIntensity = 0.2f;
    [SerializeField] private float redVignetteShowTime = 1f;
    [SerializeField] private float vignetteBlackIntensity = 0.25f;

    private Volume volume;
    private Vignette vignette;
    private Color myRed;
    private Color black = Color.black;*/

    public enum EnemyState
    {
        patrolling,
        chasing,
        attacking,
        fleeing
    }
    #endregion

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        enemyDistanceFlee = fleeRange;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        whatIsTarget = LayerMask.GetMask("Player");

        // Initialize vignette effect
        /*volume = FindObjectOfType<Volume>();
        volume.profile = volumeProfile;
        volumeProfile.TryGet(out vignette);
        if (ColorUtility.TryParseHtmlString("#E51E25", out myRed)) // Red
            vignette.color.value = myRed;*/
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform.root.gameObject;
            enemyTarget = player;
        }

        //Arm the ai with a projectile if it as a gun
        projectile = !m_IsDead || !m_IsStunned || !m_IsScared ? pistolProjectile : null;

        if (!m_IsDead && !m_IsStunned)
        {
            //Set isScared based on if the agent wields a weapon
            m_IsScared = agent.gameObject.GetComponentInChildren<ActorWeaponsManager>() == null;

            //Check for sight and attack range
            targetInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsTarget);
            targetInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsTarget);
            targetInFleeRange = Physics.CheckSphere(transform.position, fleeRange, whatIsTarget);

            // Perform a raycast to check if the target is visible (not blocked by obstacles)
            targetVisible = IsTargetVisible(enemyTarget);

            if (IsTargetVisible(player))
            {
                enemyTarget = player;
            }

            if (targetInSightRange && targetVisible) {
                if (!m_IsScared) {
                    if (!targetInAttackRange) ChaseTarget();
                    else AttackTarget();
                }
                else if (targetInFleeRange && m_IsScared) Fleeing();
            }
            else Patrolling();
        }
        else
        {
            //Handle infected state
            agent.enabled = false;
            rb.isKinematic = false;
            waypointMover.enabled = false;
        }

        if (m_IsDead)
        {
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            gameObject.GetComponent<Sliding>().enabled = false;
            gameObject.GetComponent<WaypointMover>().enabled = false;

            rb.freezeRotation = false;
            rb.isKinematic = false;
            //Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    #region SIGHT & VISIBILITY CHECK
    private bool IsTargetVisible(GameObject target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hitInfo, sightRange))
        {
            // Check if the raycast hit the player
            if (hitInfo.collider.gameObject.CompareTag("Player") || 
                hitInfo.collider.gameObject.CompareTag("Host"))
            {
                return true; // Player is visible
            }
        }
        return false; // Player is not visible (obstacle in the way)
    }
    #endregion

    #region STATE FUNCTIONS
    private void Patrolling()
    {
        state = EnemyState.patrolling;

        agent.enabled = false;
        rb.isKinematic = true;
        waypointMover.enabled = true;
    }

    private void ChaseTarget()
    {
        state = EnemyState.chasing;
        SetAgentDestination(enemyTarget.transform.position);

        agent.enabled = true;
        rb.isKinematic = false;
        waypointMover.enabled = false;
    }

    private void AttackTarget()
    {
        state = EnemyState.attacking;

        agent.enabled = true;
        rb.isKinematic = false;
        waypointMover.enabled = false;

        Debug.Log("Attacking player");

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
        whatIsTarget = LayerMask.GetMask("Host");
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
        whatIsTarget = LayerMask.GetMask("Player");
    }

    private void Fleeing()
    {
        state = EnemyState.fleeing;
        agent.speed = m_FleeSpeed;

        agent.enabled = true;
        waypointMover.enabled = false;
        rb.isKinematic = false;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < enemyDistanceFlee)
        {
            //Vector player to me
            Vector3 dirToPlayer = transform.position - enemyTarget.transform.position;

            Vector3 newPos = transform.position + dirToPlayer;
            
            SetAgentDestination(newPos);
        }
    }
    private void SetAgentDestination(Vector3 destination)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
    #endregion

    public void Stun(float stunDuration)
    {
        Debug.Log(this.name + "is stunned for" + stunDuration + " seconds");
        if (!m_IsStunned)
        {
            m_IsStunned = true;

            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            gameObject.GetComponent<WaypointMover>().enabled = false;

            StartCoroutine(EndStunAfterDuration(stunDuration));
        }
    }
    private IEnumerator EndStunAfterDuration(float stunDuration)
    {
        yield return new WaitForSeconds(stunDuration);
        m_IsStunned = false;

        gameObject.GetComponent<NavMeshAgent>().enabled = true;
        gameObject.GetComponent<WaypointMover>().enabled = true;
    }

    #region HEALTH & DMG FUNCTIONS
    public void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage called with {damage} damage.");
        if (damage <= 0) return;

        m_CurrentHealth -= damage;

        //Play hurt sound effect
        //enemyAudioHurt.Play();

        //Trigger floating text
        if (floatingText && m_CurrentHealth > 0)
            ShowFloatingText($"{damage}");

        if (m_CurrentHealth <= 0)
        {
            m_IsDead = true;
        }

        // Show vignette effect on hit
        //StartCoroutine(ShowVignetteOnHitCoroutine());
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    private void ShowFloatingText(string textToShow)
    {
        //Can be optimized by Object Pooling
        var go = Instantiate(floatingText, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMeshPro>().text = textToShow;
    }
    /*private IEnumerator ShowVignetteOnHitCoroutine()
    {
        float originalIntensity = vignette.intensity.value;

        // Set vignette to red
        vignette.color.value = myRed;
        vignette.intensity.value = vignetteRedIntensity;

        // Wait for the specified time
        yield return new WaitForSeconds(redVignetteShowTime);

        // Gradually return to original intensity
        while (vignette.intensity.value > originalIntensity)
        {
            vignette.intensity.value -= 0.01f;
            yield return new WaitForSeconds(0.1f);
        }

        // Reset vignette to black if still at low health
        vignette.intensity.value = vignetteBlackIntensity;
        vignette.color.value = black;
    }*/

    private void OnParticleCollision(GameObject other)
    {
        TakeDamage(1);
        Stun(5f);
    }
    #endregion

    #region GIZMOS
    /// <summary>
    /// Visualize attack and sight range.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
    }
    #endregion
}
