using UnityEngine;
/*
public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public int targetPoint;
    private float speed;

    private EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();

        speed = enemyAI.patrolSpeed;
        targetPoint = 0;
    }

    private void Update()
    {
        Patrolling();
    }

    public void Patrolling()
    {
        if (transform.position == patrolPoints[targetPoint].position)
        {
            IncreaseTargetInt();
        }
        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[targetPoint].position, speed * Time.deltaTime);
    }

    void IncreaseTargetInt()
    {
        targetPoint++;
        if(targetPoint >= patrolPoints.Length)
        {
            targetPoint = 0;
        }
    }
} */
