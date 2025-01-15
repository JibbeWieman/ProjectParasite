//using UnityEngine;

//public class m_PatrolAgent : MonoBehaviour
//{
//    #region VARIABLES
//    //Stores a reference to the waypoint system this object will use
//    [SerializeField] private Waypoints waypoints;
//    [SerializeField] private string waypointObjectName;

//    [SerializeField] private float moveSpeed = 5f;

//    [Range(0f, 15f)] //How fast the agent will rotate once it reaches its waypoint
//    [SerializeField] private float rotateSpeed = 10f;

//    [SerializeField] private float distanceThreshold = 0.1f;

//    //The current waypoint target that the object is moving towards
//    private Transform currentWaypoint;


//    //The rotation target for the current frame
//    private Quaternion rotationGoal;
//    //The direction to the next waypoint that the agent needs to rotate towards
//    private Vector3 directionToWaypoint;

//    [Header("Patrolling")]
//    public Vector3 walkPoint;
//    bool walkPointSet;
//    public float walkPointRange;
//    public int targetPoint;
//    #endregion

//    private void Awake()
//    {
//        waypoints = GameObject.Find(waypointObjectName).GetComponent<Waypoints>();

//        moveSpeed = gameObject.GetComponent<EnemyAI>().PatrolSpeed;
//    }

//    void Start()
//    {
//        //Set initial position to the first waypoint
//        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
//        transform.position = currentWaypoint.position;

//        //Set the next waypoint target
//        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
//        transform.LookAt(currentWaypoint);
//    }

//    void Update()
//    {
//        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
//        if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
//        {
//            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
//        }
//        RotateTowardsWaypoint();

//        //If there host is taken to a level where he can't access its waypoints move randomly
//        if (waypoints == null)
//        {
//            Pacing();
//        }
//    }

//    //Will slowly rotate the agent towards the current waypoint it is moving towards
//    private void RotateTowardsWaypoint()
//    {
//        directionToWaypoint = (currentWaypoint.position - transform.position).normalized;
//        rotationGoal = Quaternion.LookRotation(directionToWaypoint);
//        transform.rotation = Quaternion.Slerp(transform.rotation, rotationGoal, rotateSpeed * Time.deltaTime);
//    }

//    #region PACING FUNCTIONS
//    private void Pacing()
//    {
//        if (!walkPointSet) SearchWalkPoint();

//        if (walkPointSet)
//            transform.position = Vector3.MoveTowards(transform.position, walkPoint, moveSpeed * Time.deltaTime);

//        Vector3 distanceToWalkPoint = transform.position - walkPoint;

//        //Walkpoint reached
//        if (distanceToWalkPoint.magnitude < 1f)
//            walkPointSet = false;
//    }
//    private void SearchWalkPoint()
//    {
//        //Calculate random point in range
//        float randomZ = Random.Range(-walkPointRange, walkPointRange);
//        float randomX = Random.Range(-walkPointRange, walkPointRange);

//        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

//        if (Physics.Raycast(walkPoint, -transform.up, 2f, LayerMask.NameToLayer("Ground")))
//            walkPointSet = true;
//    }
//    #endregion
//}
