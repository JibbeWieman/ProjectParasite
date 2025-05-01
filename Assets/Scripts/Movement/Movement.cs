using UnityEngine;
using System.Collections;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    private Rigidbody RB;

    float moveSpeed = 2;
    float rotationSpeed = 4;
    float runningSpeed;
    float vaxis, haxis;
    public bool isJumping, isJumpingAlt, isGrounded = false;
    Vector3 movement;

    bool IsLeeching = false;
    public CinemachineFreeLook cam;

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Debug.Log("Initialized: (" + this.name + ")");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Leeching());
        }
    }

    void FixedUpdate()
    {
        /*  Controller Mappings */
        vaxis = Input.GetAxis("Vertical");
        haxis = Input.GetAxis("Horizontal");
        isJumping = Input.GetButton("Jump");
        isJumpingAlt = Input.GetKey(KeyCode.Joystick1Button0);

        //Simplified...
        runningSpeed = vaxis;

        if (isGrounded)
        {
            movement = new Vector3(0, 0f, runningSpeed * 8);        // Multiplier of 8 seems to work well with Rigidbody Mass of 1.
            movement = transform.TransformDirection(movement);      // transform correction A.K.A. "Move the way we are facing"
        }
        else
        {
            movement *= 0.70f;                                      // Dampen the movement vector while mid-air
        }

        RB.AddForce(movement * moveSpeed);   // Movement Force

        if ((isJumping || isJumpingAlt) && isGrounded)
        {
            Debug.Log(this.ToString() + " isJumping = " + isJumping);
            RB.AddForce(Vector3.up * 150);
        }

        if ((Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) && !isJumping && isGrounded)
        {
            if (Input.GetAxis("Vertical") >= 0)
                transform.Rotate(new Vector3(0, haxis * rotationSpeed, 0));
            else
                transform.Rotate(new Vector3(0, -haxis * rotationSpeed, 0));
        }
    }

    IEnumerator Leeching()
    {
        IsLeeching = true;
        Debug.Log("Leeching started");

        yield return new WaitForSeconds(3);

        IsLeeching = false;
        Debug.Log("Leeching ended");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exited");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
