using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    #region Variables
    public bool canMove = true;
    public bool isMoving = false;

    [SerializeField] private float speed;
    public int startPoint;
    public Transform[] points;

    private int i;
    private bool reverse;
    #endregion

    private void Start()
    {
        transform.position = points[startPoint].position;
        i = startPoint;
    }

    private void Update()
    {
        // Check if platform reached the target point
        if (Vector3.Distance(transform.position, points[i].position) < 0.01f)
        {
            canMove = false;
            isMoving = false; // Not moving anymore

            // Set next target point
            if (i == points.Length - 1)
            {
                reverse = true;
                i--;
                return;
            }
            else if (i == 0)
            {
                reverse = false;
                i++;
                return;
            }

            // Adjust target point index
            i = reverse ? i - 1 : i + 1;
        }

        // Move the platform
        if (canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }
    /* #region Variables
    [SerializeField] private float speed;
    [SerializeField] private Transform[] points;
    [SerializeField] private int startIndex;

    private int targetIndex;
    private bool reverse;
    public bool isMoving { get; set; }
    #endregion

    private void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogError("No points assigned to the moving platform.");
            enabled = false;
            return;
        }

        if (points.Length == 1)
        {
            transform.position = points[0].position;
            return;
        }

        targetIndex = Mathf.Clamp(startIndex, 0, points.Length - 1);
        transform.position = points[targetIndex].position;
    }

    private void Update()
    {
        if (isMoving)
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[targetIndex].position, speed * Time.deltaTime);

        // Stop moving if destination reached
        if (Mathf.Approximately(Vector3.Distance(transform.position, points[targetIndex].position), 0f))
        {
            isMoving = false;
            SetNextTarget();
        }
    }

    private void SetNextTarget()
    {
        reverse = (targetIndex == points.Length - 1) ? true : (targetIndex == 0 ? false : reverse);
        targetIndex += reverse ? -1 : 1;
        isMoving = true; // Start moving again
    } */
}


// Old tutorial code