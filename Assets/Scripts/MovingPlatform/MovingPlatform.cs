using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public bool canMove = true;
    public bool isMoving = false;

    [SerializeField] float speed;
    public int startPoint;
    public Transform[] points;

    private int i;
    public bool reverse;

    //private TriggerPlatform triggerPlatform;

    private void Start()
    {
        transform.position = points[startPoint].position;
        i = startPoint;
        //triggerPlatform = GetComponent<TriggerPlatform>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, points[i].position) < 0.01f)
        {
            canMove = false;

            //set next target point
            if (i == points.Length - 1)
            {
                // Start the coroutine to delay the movement
                //StartCoroutine(triggerPlatform.StartMovementAfterDelay());

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

            //target point counter
            if (reverse)
            {
                i--;
            }
            else
            {
                i++;
            }
        }

        //move
        if (canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
        }

    }
}
