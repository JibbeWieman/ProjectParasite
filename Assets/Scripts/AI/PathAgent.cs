using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moves an agent along a patrol path with optional looping and random fallback movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PatrolAgent : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("The patrol path this agent will follow.")]
    private PatrolPath patrolPath;

    [SerializeField, Tooltip("Should the agent loop the path?")]
    private bool loop = true;

    [SerializeField, Tooltip("Speed of the agent.")]
    private float speed = 2f;

    [SerializeField, Tooltip("Rotation speed of the agent.")]
    private float rotationSpeed = 5f;

    [SerializeField, Tooltip("Fallback random pacing range.")]
    private float randomPaceRange = 5f;

    private int currentWaypointIndex = 0;
    private bool isReversing = false;
    private CharacterController characterController;

    #endregion

    #region Unity Methods

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (patrolPath == null || patrolPath.NodeCount == 0)
        {
            Debug.LogWarning($"{name} has no patrol path assigned. Switching to random pacing.");
        }
    }

    private void Update()
    {
        if (patrolPath != null && patrolPath.NodeCount > 0)
        {
            Patrol();
        }
        else
        {
            RandomPacing();
        }
    }

    #endregion

    #region Patrol Logic

    private void Patrol()
    {
        Vector3 targetPosition = patrolPath.GetPositionOfWaypoint(currentWaypointIndex);
        MoveTowards(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        if (loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPath.NodeCount;
        }
        else
        {
            if (isReversing)
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    isReversing = false;
                }
            }
            else
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= patrolPath.NodeCount)
                {
                    currentWaypointIndex = patrolPath.NodeCount - 2;
                    isReversing = true;
                }
            }
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        characterController.Move(direction * speed * Time.deltaTime);

        // Smooth rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #endregion

    #region Random Pacing

    private void RandomPacing()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-randomPaceRange, randomPaceRange),
            transform.position.y,
            Random.Range(-randomPaceRange, randomPaceRange));

        MoveTowards(randomPosition);
    }

    #endregion
}
