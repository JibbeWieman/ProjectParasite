using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HostCharacterController : ActorCharacterController
{
    [SerializeField] protected Animator m_AnimatorDead;
    [SerializeField] protected GameObject aliveModel;
    [SerializeField] protected GameObject deadModel;
    ActorWeaponsManager m_WeaponsManager;

    public float RotationMultiplier
    {
        get
        {
            if (m_WeaponsManager != null && Events.AimEvent.IsAiming)
            {
                return AimingRotationMultiplier;
            }

            return 1f;
        }
    }

    protected override void Start()
    {
        Debug.Log($"Before base.Start(): {transform.position}");
        base.Start();
        Debug.Log($"After base.Start(): {transform.position}");

        m_WeaponsManager = GetComponent<ActorWeaponsManager>();
        DebugUtility.HandleErrorIfNullGetComponent<ActorWeaponsManager, ActorCharacterController>(
            m_WeaponsManager, this, gameObject);

        m_PatrolAgent = GetComponent<PatrolAgent>();
        DebugUtility.HandleErrorIfNullGetComponent<PatrolAgent, ActorCharacterController>(m_PatrolAgent,
            this, gameObject);

        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        DebugUtility.HandleErrorIfNullGetComponent<NavMeshAgent, ActorCharacterController>(m_NavMeshAgent,
            this, gameObject);

        // Ensure models and animation states are properly set
        deadModel?.SetActive(false);
        m_Animator.SetBool("isWalking", true);
    }


    protected override void OnDie()
    {
        base.OnDie();

        m_Animator = m_AnimatorDead;

        aliveModel.SetActive(false);
        deadModel.SetActive(true);

        //m_WeaponsManager.SwitchToWeaponIndex(-1, true); // Tell the weapons manager to switch to a non-existing weapon in order to lower the weapon
    }

    public override void AdjustAnimationSpeed()
    {
        if (m_Animator != null)
        {
            if (m_InputHandler?.GetHorizontalInput() == 0 && m_InputHandler?.GetVerticalInput() == 0)
            {
                m_DesiredAnimationSpeed = 0f;
            }
            else
            {
                m_DesiredAnimationSpeed = Mathf.Clamp(CharacterVelocity.magnitude, 1f, 3f);
                m_Animator.SetBool("isWalking", true);
            }

            m_CurrentAnimationSpeed = Mathf.Lerp(m_CurrentAnimationSpeed, m_DesiredAnimationSpeed, m_TransitionSpeed);
            m_Animator.speed = m_CurrentAnimationSpeed;
        }
    }

}
