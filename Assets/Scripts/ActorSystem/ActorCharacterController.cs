using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
public class ActorCharacterController : MonoBehaviour
{
    #region VARIABLES
    protected static ActorCharacterController s_Instance;
    public static ActorCharacterController Instance { get { return s_Instance; } }
    
    [Header("References")]
    [Tooltip("Reference to the main camera used for the actor")]
    public Camera ActorCamera;

    [Tooltip("Audio source for footsteps, jump, etc...")]
    public AudioSource AudioSource;

    public AbilityBase[] m_Abilities;

    [Header("General")]
    [Tooltip("Force applied downward when in the air")]
    public float GravityDownForce = 20f;

    [Tooltip("Physic layers checked to consider the actor grounded")]
    public LayerMask GroundCheckLayers = -1;

    [Tooltip("Distance from the bottom of the character controller capsule to test for grounded")]
    public float GroundCheckDistance = 0.05f;

    [Header("Movement")]
    [Tooltip("The direction the gameobject is oriented towards")]
    public Transform orientation;

    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float MaxSpeedOnGround = 10f;

    [Tooltip(
        "Sharpness for the movement when grounded, a low value will make the actor accelerate and decelerate slowly, a high value will do the opposite")]
    public float MovementSharpnessOnGround = 15;

    [Tooltip("Max movement speed when crouching")]
    [Range(0, 1)]
    public float MaxSpeedCrouchedRatio = 0.5f;

    [Tooltip("Max movement speed when not grounded")]
    public float MaxSpeedInAir = 10f;

    [Tooltip("Acceleration speed when in the air")]
    public float AccelerationSpeedInAir = 25f;

    [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
    public float SprintSpeedModifier = 2f;

    [Tooltip("Height at which the actor dies instantly when falling off the map")]
    public float KillHeight = -50f;

    [Header("Rotation")]
    [Tooltip("Rotation speed for moving the camera")]
    public float RotationSpeed = 200f;

    [Range(0.1f, 1f)]
    [Tooltip("Rotation speed multiplier when aiming")]
    public float AimingRotationMultiplier = 0.4f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    public float JumpForce = 9f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    protected bool toggleCrouch = false;

    [Header("Stance")]
    [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
    public float CameraHeightRatio = 0.9f;

    [Tooltip("Height of character when standing")]
    public float CapsuleHeightStanding = 1.2f;

    [Tooltip("Height of character when standing")]
    [SerializeField]
    protected float CapsuleScaleStanding = 1f;

    [Tooltip("Height of character when crouching")]
    public float CapsuleHeightCrouching = 0.9f;

    [Tooltip("Height of character when crouching")]
    [SerializeField]
    protected float CapsuleScaleCrouching = 0.8f;

    [Tooltip("Speed of crouching transitions")]
    public float CrouchingSharpness = 10f;

    [Header("Audio")]
    [Tooltip("Amount of footstep sounds played when moving one meter")]
    public float FootstepSfxFrequency = 1f;

    [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
    public float FootstepSfxFrequencyWhileSprinting = 1f;

    [Tooltip("Sound played for footsteps")]
    public AudioClip[] FootstepSfx;

    [Tooltip("Sound played when jumping")] public AudioClip[] JumpSfx;
    [Tooltip("Sound played when landing")] public AudioClip[] LandSfx;

    [Tooltip("Sound played when taking damage froma fall")]
    public AudioClip FallDamageSfx;

    [Header("Fall Damage")]
    [Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
    public bool RecievesFallDamage;

    [Tooltip("Minimun fall speed for recieving fall damage")]
    public float MinSpeedForFallDamage = 10f;

    [Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
    public float MaxSpeedForFallDamage = 30f;

    [Tooltip("Damage recieved when falling at the mimimum speed")]
    public float FallDamageAtMinSpeed = 10f;

    [Tooltip("Damage recieved when falling at the maximum speed")]
    public float FallDamageAtMaxSpeed = 50f;

    [Header("Animation")]
    [SerializeField] protected Animator m_AnimatorAlive;
    [Range(0.001f, 1f)][SerializeField] protected float m_TransitionSpeed = 0.06f;
    protected float m_DesiredAnimationSpeed;
    protected float m_CurrentAnimationSpeed;

    public UnityAction<bool> OnStanceChanged;

    public Vector3 CharacterVelocity { get; set; }
    public bool IsGrounded { get; protected set; }
    public bool HasJumpedThisFrame { get; protected set; }
    public bool IsDead;
    [SerializeField] protected bool VisiblyDead = false;
    public bool IsCrouching { get; protected set; }

    protected Actor m_Actor;
    protected Animator m_Animator;
    protected Health m_Health;
    protected PlayerInputHandler m_InputHandler;
    protected CharacterController m_Controller;

    protected Vector3 m_GroundNormal;
    //Vector3 m_CharacterVelocity;
    protected Vector3 m_LatestImpactSpeed;
    protected float m_LastTimeJumped = 0f;
    //float m_CameraVerticalAngle = 0f;
    protected float m_FootstepDistanceCounter;
    protected float m_TargetCharacterHeight;
    protected bool isSprinting;

    protected const float k_JumpGroundingPreventionTime = 0.2f;
    protected const float k_GroundCheckDistanceInAir = 0.07f;
    #endregion

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            Destroy(other.gameObject);
        }
    }

    protected virtual void Start()
    {
        s_Instance = this;
        if (m_Abilities != null)
        {
            m_Abilities = GetComponents<AbilityBase>();
        }
        ActorCamera = Camera.main;

        m_Controller = GetComponent<CharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<CharacterController, ActorCharacterController>(m_Controller,
            this, gameObject);

        m_InputHandler = GetComponent<PlayerInputHandler>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, ActorCharacterController>(m_InputHandler,
            this, gameObject);

        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, ActorCharacterController>(m_Health, this, gameObject);

        m_Actor = GetComponent<Actor>();
        DebugUtility.HandleErrorIfNullGetComponent<Actor, ActorCharacterController>(m_Actor, this, gameObject);

        m_Controller.enableOverlapRecovery = true;

        m_Health.OnDie += OnDie;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);

        m_Animator = m_AnimatorAlive;

        Debug.Log("Start(): FootstepSfx Length = " + FootstepSfx.Length);
        for (int i = 0; i < FootstepSfx.Length; i++)
        {
            Debug.Log($"Start(): FootstepSfx[{i}] = {FootstepSfx[i]}");
        }
    }

    protected virtual void Update()
    {
        // check for Y kill
        if (!IsDead && transform.position.y < KillHeight)
        {
            m_Health.Kill();
        }

        HasJumpedThisFrame = false;

        bool wasGrounded = IsGrounded;
        GroundCheck();

        // landing
        if (IsGrounded && !wasGrounded)
        {
            // Fall damage
            float fallSpeed = -Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y);
            float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) /
                                    (MaxSpeedForFallDamage - MinSpeedForFallDamage);
            if (RecievesFallDamage && fallSpeedRatio > 0f)
            {
                float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                m_Health.TakeDamage(dmgFromFall, null);

                // fall damage SFX
                AudioSource.PlayOneShot(FallDamageSfx);
            }
            else
            {
                // land SFX
                if (AudioSource.clip != LandSfx[0])
                    Game_Manager.PlayRandomSfx(AudioSource, LandSfx);
            }
        }

        if (IsGrounded) 
        {
            HandleFootsteps();
        }

        UpdateCharacterHeight(false);
    }

    public virtual void AdjustAnimationSpeed()
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

    protected virtual void OnDie()
    {
        IsDead = true;
        VisiblyDead = true;
    }

    protected virtual void GroundCheck()
    {
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance =
            IsGrounded ? (m_Controller.skinWidth + GroundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        IsGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height),
                m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, GroundCheckLayers,
                QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    IsGrounded = true;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
        Debug.DrawRay(transform.position, Vector3.down * chosenGroundCheckDistance, Color.red);
    }

    public virtual void HandleCharacterMovement()
    {
        // character movement handling
        HandleSprinting();

        float speedModifier = isSprinting ? SprintSpeedModifier : 1f; //* MovementSpeedModifier;

        // calculate movement direction
        Vector3 moveDirection = orientation.forward * Input.GetAxis("Vertical") +
                                orientation.right * Input.GetAxis("Horizontal");
        moveDirection.Normalize(); // Normalise to ensure consistent magnitude

        // handle grounded movement
        if (IsGrounded)
        {
            // calculate the desired velocity from inputs, max speed, and current slope
            Vector3 targetVelocity = MaxSpeedOnGround * speedModifier * moveDirection;
            // reduce speed if crouching by crouch speed ratio
            if (IsCrouching)
                targetVelocity *= MaxSpeedCrouchedRatio;

            // reorient velocity on slope
            targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) *
                                targetVelocity.magnitude;

            // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
            CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                MovementSharpnessOnGround * Time.deltaTime);
            //CharacterVelocity = Vector3.MoveTowards(CharacterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);

            HandleJump();
        }
        // handle air movement
        else
        {
            // add air acceleration
            CharacterVelocity += AccelerationSpeedInAir * Time.deltaTime * moveDirection;

            // limit air speed to a maximum, but only horizontally
            float verticalVelocity = CharacterVelocity.y;
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);
            CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

            // apply the gravity to the velocity
            CharacterVelocity += GravityDownForce * Time.deltaTime * Vector3.down;
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        m_Controller.Move(CharacterVelocity * Time.deltaTime);

        // detect obstructions to adjust velocity accordingly
        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius,
            CharacterVelocity.normalized, out RaycastHit hit, CharacterVelocity.magnitude * Time.deltaTime, -1,
            QueryTriggerInteraction.Ignore))
        {
            // We remember the last impact speed because the fall damage logic might need it
            m_LatestImpactSpeed = CharacterVelocity;

            CharacterVelocity = Vector3.ProjectOnPlane(CharacterVelocity, hit.normal);
        }
    }

    protected virtual void HandleFootsteps()
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

                Game_Manager.PlayRandomSfx(AudioSource, FootstepSfx, .4f);
            }
        }
    }

    protected void HandleSprinting()
    {
        isSprinting = (bool)(m_InputHandler?.GetSprintInputHeld());
        if (isSprinting)
        {
            isSprinting = SetCrouchingState(false, false);
        }
    }

    protected void HandleJump()
    {
        if (IsGrounded && (bool)(m_InputHandler?.GetJumpInputDown()))
        {
            Debug.Log("Trying to jump 2");
            // force the crouch state to false
            if (SetCrouchingState(false, false) || SetCrouchingState(false, true))
            {
                Debug.Log("Trying to jump 3");
                // start by canceling out the vertical component of our velocity
                CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);

                // then, add the jumpSpeed value upwards
                CharacterVelocity += Vector3.up * JumpForce;

                // play sound
                Game_Manager.PlayRandomSfx(AudioSource, JumpSfx);

                // remember last time we jumped because we need to prevent snapping to ground for a short time
                m_LastTimeJumped = Time.time;
                HasJumpedThisFrame = true;

                // Force grounding to false
                IsGrounded = false;
                m_GroundNormal = Vector3.up;
            }
        }
    }

    // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
    protected bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    protected Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    protected Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    // Gets a reoriented direction that is tangent to a given slope
    public virtual Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    protected virtual void UpdateCharacterHeight(bool force)
    {
        // Update height instantly
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = 0.5f * m_Controller.height * Vector3.up;
            ActorCamera.transform.localPosition = CameraHeightRatio * m_TargetCharacterHeight * Vector3.up;
            m_Actor.AimPoint.transform.localPosition = m_Controller.center;
        }
        // Update smooth height
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            // resize the capsule and adjust camera position
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight,
                CrouchingSharpness * Time.deltaTime);
            m_Controller.center = 0.5f * m_Controller.height * Vector3.up;
            ActorCamera.transform.localPosition = Vector3.Lerp(ActorCamera.transform.localPosition,
                CameraHeightRatio * m_TargetCharacterHeight * Vector3.up, CrouchingSharpness * Time.deltaTime);
            m_Actor.AimPoint.transform.localPosition = m_Controller.center;
        }
    }

    // returns false if there was an obstruction
    protected virtual bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // set appropriate heights
        if (crouched)
        {
            m_TargetCharacterHeight = CapsuleHeightCrouching;
        }
        else
        {
            // Detect obstructions
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(CapsuleHeightStanding),
                    m_Controller.radius,
                    -1,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = CapsuleHeightStanding;
        }

        OnStanceChanged?.Invoke(crouched);

        IsCrouching = crouched;
        return true;
    }
}