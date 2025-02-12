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
    private EnemyAI m_EnemyAI;

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
        //Debug.Log($"Before base.Start(): {transform.position}");
        base.Start();
        //Debug.Log($"After base.Start(): {transform.position}");

        m_WeaponsManager = GetComponent<ActorWeaponsManager>();
        DebugUtility.HandleErrorIfNullGetComponent<ActorWeaponsManager, ActorCharacterController>(
            m_WeaponsManager, this, gameObject);

        m_EnemyAI = GetComponent<EnemyAI>();
        DebugUtility.HandleErrorIfNullGetComponent<EnemyAI, ActorCharacterController>(m_EnemyAI,
            this, gameObject);

        // Ensure models and animation states are properly set
        deadModel?.SetActive(false);
        m_Animator.SetBool("isWalking", true);
    }

    protected override void Update()
    {
        base.Update();

        if (!IsGrounded && IsDead && !m_Actor.IsActive())
        {
            CharacterVelocity += GravityDownForce * Time.deltaTime * Vector3.down;
            m_Controller.Move(CharacterVelocity * Time.deltaTime);
            return; // Prevent movement logic but allow gravity
        }
    }


    protected override void OnDie()
    {
        base.OnDie();

        m_Animator = m_AnimatorDead;

        aliveModel.SetActive(false);
        deadModel.SetActive(true);

        m_EnemyAI.enabled = false;

        m_TargetCharacterHeight = CapsuleHeightCrouching;

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

    public override void HandleCharacterMovement()
    {
        base.HandleCharacterMovement();

        HandleCrouching();
    }

    protected void HandleCrouching()
    {
        if (!IsGrounded || IsDead || VisiblyDead) return;

        if (toggleCrouch)
        {
            if (m_InputHandler.GetCrouchInputDown())
            {
                IsCrouching = !IsCrouching;
                SetCrouchingState(IsCrouching, false);
                m_TargetCharacterHeight = IsCrouching ? CapsuleHeightCrouching : CapsuleHeightStanding;
            }
        }
        else
        {
            // Ensure crouch state only changes when the key is explicitly released
            bool crouchHeld = m_InputHandler.GetCrouchInputHeld();
            bool crouchReleased = m_InputHandler.GetCrouchInputReleased(); // Explicit check for key release

            if (crouchHeld && !IsCrouching)
            {
                // Start crouching
                SetCrouchingState(true, false);
                IsCrouching = true;
                m_TargetCharacterHeight = CapsuleHeightCrouching;
            }
            else if (!crouchHeld && crouchReleased && IsCrouching)
            {
                // Ensure standing up happens only when the key is actually released
                SetCrouchingState(false, false);
                IsCrouching = false;
                m_TargetCharacterHeight = CapsuleHeightStanding;
            }
        }

        // Smoothly transition to target height
        m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, CrouchingSharpness * Time.deltaTime);
        transform.localScale = new Vector3(
            transform.localScale.x,
            Mathf.Lerp(transform.localScale.y, IsCrouching ? CapsuleScaleCrouching : CapsuleScaleStanding, CrouchingSharpness * Time.deltaTime),
            transform.localScale.z
        );
    }


    protected override void HandleFootsteps()
    {
        if (FootstepSfx.Length > 0)
        {
            // Update the footstep counter based on movement speed
            m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;

            // Footsteps sound logic
            float chosenFootstepSfxFrequency =
                (isSprinting ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);

            if (m_FootstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
            {
                m_FootstepDistanceCounter = 0f;

                Game_Manager.PlayRandomSfx(AudioSource, FootstepSfx, 0.02f);
            }
        }
    }
}
