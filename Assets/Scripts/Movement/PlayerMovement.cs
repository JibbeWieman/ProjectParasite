using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    #region VARIABLES
    protected static PlayerMovement s_Instance;
    public static PlayerMovement Instance { get { return s_Instance; } }
    public bool Respawning { get { return m_Respawning; } }

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;                         // How fast the player takes off when jumping.
    public float jumpCooldown;
    public float airMultiplier;
    protected bool m_ReadyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit m_SlopeHit;
    private bool m_ExitingSlope;

    [Header("Animation")]
    protected Animator m_Animator;
    private float m_DesiredAnimationSpeed;
    private float m_CurrentAnimationSpeed;
    [Range(0.001f, 5f)] [SerializeField] private float m_TransitionSpeed = 0.1f; 

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    [HideInInspector] public Rigidbody rb;
    public AbilityBase[] m_Abilities;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    protected bool m_Respawning;                   // Whether the player is currently respawning.

    #endregion

    private void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
        if (actorsManager != null)
            actorsManager.SetPlayer(gameObject);
    }

    private void Start()
    {
        rb.freezeRotation = true;

        m_ReadyToJump = true;

        startYScale = transform.localScale.y;

        m_Abilities = GetComponents<AbilityBase>();

        s_Instance = this;
    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        DrawGroundedRaycast(); // Visualize GroundedRaycast

        MyInput();
        SpeedControl();
        StateHandler();

        //handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void DrawGroundedRaycast()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;
        float rayLength = playerHeight * 0.5f + 0.2f;

        // Draw the raycast in red if not grounded, green if grounded
        Color rayColor = grounded ? Color.green : Color.red;
        Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * rayLength, rayColor);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        AdjustAnimationSpeed();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //when to jump
        if(Input.GetKey(jumpKey) && m_ReadyToJump && grounded)
        {
            m_ReadyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        //Mode - Sliding
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else 
                desiredMoveSpeed = sprintSpeed;
        }

        //Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        //Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        //Mode - Sprinting
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        //Mode - Air
        else
        {
            state = MovementState.air;
        }

        //check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, m_SlopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on slope
        if (OnSlope() && !m_ExitingSlope)
        {
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);

            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //on ground
        if (grounded)
        {
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }

        //in air
        else if(!grounded)
        {
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }

        //turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //limiting speed on slope
        if (OnSlope() && !m_ExitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        //limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);

            //limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void AdjustAnimationSpeed()
    {
        if (m_Animator != null)
        {
            if (horizontalInput == 0 && verticalInput == 0)
            {
                m_DesiredAnimationSpeed = 0f;
            }
            else
            {
                m_DesiredAnimationSpeed = Mathf.Clamp(moveSpeed * 0.7f, 1f, 7f);
            }

            m_CurrentAnimationSpeed = Mathf.Lerp(m_CurrentAnimationSpeed, m_DesiredAnimationSpeed, m_TransitionSpeed);
            m_Animator.speed = m_CurrentAnimationSpeed;
        }
    }

    private void Jump()
    {
        m_ExitingSlope = true;

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        m_ReadyToJump = true;

        m_ExitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out m_SlopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, m_SlopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, m_SlopeHit.normal).normalized;
    }
}