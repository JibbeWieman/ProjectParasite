using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DetectionModule : MonoBehaviour
{
    [Tooltip("The point representing the source of target-detection raycasts for the enemy AI")]
    public Transform DetectionSourcePoint;

    [Tooltip("The max distance at which the enemy can see targets")]
    public float DetectionRange = 20f;

    [Tooltip("The max distance at which the enemy can attack its target")]
    public float AttackRange = 10f;

    [Tooltip("Time before an enemy abandons a known target that it can't see anymore")]
    public float KnownTargetTimeout = 4f;

    [Tooltip("Optional animator for OnShoot animations")]
    private Animator Animator;

    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;

    public GameObject KnownDetectedTarget { get; private set; }
    public bool IsTargetInAttackRange { get; private set; }
    public bool IsTargetInDetectionRange { get; private set; }
    public bool IsSeeingTarget { get; private set; }
    public bool HadKnownTarget { get; private set; }
    public bool SpottedDeadBody { get; private set; }

    protected float TimeLastSeenTarget = Mathf.NegativeInfinity;

    ActorsManager m_ActorsManager;

    const string k_AnimAttackParameter = "Attack";
    const string k_AnimOnDamagedParameter = "OnDamaged";

    protected virtual void Start()
    {
        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, DetectionModule>(m_ActorsManager, this);
    }

    public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
    {
        // Handle known target detection timeout
        if (KnownDetectedTarget && !IsSeeingTarget && (Time.time - TimeLastSeenTarget) > KnownTargetTimeout)
        {
            KnownDetectedTarget = null;
        }

        // Find the closest visible hostile actor or detect dead bodies with the same affiliation
        float sqrDetectionRange = DetectionRange * DetectionRange;
        IsSeeingTarget = false;
        float closestSqrDistance = Mathf.Infinity;
        IsTargetInDetectionRange = false; // Reset each frame

        bool spottedDeadBody = false; // Variable for detecting dead bodies

        foreach (Actor otherActor in m_ActorsManager.Actors)
        {
            float sqrDistance = (otherActor.transform.position - DetectionSourcePoint.position).sqrMagnitude;

            // Detect dead bodies with the same affiliation
            if (otherActor.Affiliation == actor.Affiliation && otherActor.CompareTag("DeadHost"))
            {
                if (sqrDistance < sqrDetectionRange)
                {
                    // Check for obstructions
                    RaycastHit[] hits = Physics.RaycastAll(DetectionSourcePoint.position,
                        (otherActor.transform.position - DetectionSourcePoint.position).normalized, DetectionRange,
                        -1, QueryTriggerInteraction.Ignore);
                    bool isVisible = hits.All(hit => selfColliders.Contains(hit.collider) || hit.collider.GetComponentInParent<Actor>() == otherActor);

                    if (isVisible && !spottedDeadBody)
                    {
                        spottedDeadBody = true;
                        EventManager.Broadcast(Events.OnBodyFoundEvent); // Invoke the method when a dead body is spotted
                    }
                }
            }

            // Detect hostile targets
            if (otherActor.Affiliation != actor.Affiliation)
            {
                if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                {
                    // Check for obstructions
                    RaycastHit[] hits = Physics.RaycastAll(DetectionSourcePoint.position,
                        (otherActor.AimPoint.position - DetectionSourcePoint.position).normalized, DetectionRange,
                        -1, QueryTriggerInteraction.Ignore);
                    RaycastHit closestValidHit = new RaycastHit();
                    closestValidHit.distance = Mathf.Infinity;
                    bool foundValidHit = false;
                    foreach (var hit in hits)
                    {
                        if (!selfColliders.Contains(hit.collider) && hit.distance < closestValidHit.distance)
                        {
                            closestValidHit = hit;
                            foundValidHit = true;
                        }
                    }

                    if (foundValidHit)
                    {
                        Actor hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                        if (hitActor == otherActor)
                        {
                            IsSeeingTarget = true;
                            IsTargetInDetectionRange = true; // Set to true when target is in detection range
                            closestSqrDistance = sqrDistance;

                            TimeLastSeenTarget = Time.time;
                            KnownDetectedTarget = otherActor.AimPoint.gameObject;
                        }
                    }
                }
            }
        }

        IsTargetInAttackRange = KnownDetectedTarget != null &&
                                Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= AttackRange;

        // Detection events
        if (!HadKnownTarget &&
            KnownDetectedTarget != null)
        {
            OnDetect();
        }

        if (HadKnownTarget &&
            KnownDetectedTarget == null)
        {
            OnLostTarget();
        }

        // Remember if we already knew a target (for next frame)
        HadKnownTarget = KnownDetectedTarget != null;
    }


    public virtual void OnLostTarget() => onLostTarget?.Invoke();

    public virtual void OnDetect() => onDetectedTarget?.Invoke();

    public virtual void OnDamaged(GameObject damageSource)
    {
        TimeLastSeenTarget = Time.time;
        KnownDetectedTarget = damageSource;

        if (Animator)
        {
            Animator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }

    public virtual void OnAttack()
    {
        if (Animator)
        {
            Animator.SetTrigger(k_AnimAttackParameter);
        }
    }
}
