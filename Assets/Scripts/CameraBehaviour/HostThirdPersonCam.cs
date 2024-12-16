using UnityEngine;
using Cinemachine;

public class HostThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The rotation speed of the host character.")]
    private float rotationSpeed;
    [SerializeField]
    private Transform Orientation;
    [SerializeField]
    private Transform CombatLookAt;
    [SerializeField]
    private Transform hostObj;
    [SerializeField]
    private CharacterController cc;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat
    }

    public void SetupHostVariables(Actor hostActor)
    {
        hostObj = hostActor.transform.Find("Model");
        Orientation = hostActor.transform.Find("Orientation");
        CombatLookAt = Orientation?.Find("CombatLookAt");
        cc = hostActor.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (InfectAbility.inHost && hostObj != null)
        {
            //HandleCameraSwitching();
            HandleHostRotation();
        }
        else
        {
            ResetHostVariables();
        }
    }

    //private void HandleCameraSwitching()
    //{
    //    if (Input.GetKeyUp(KeyCode.Mouse1))
    //    {
    //        SwitchCameraStyle(CameraStyle.Basic);
    //    }

    //    if (Input.GetKeyDown(KeyCode.Mouse1))
    //    {
    //        SwitchCameraStyle(CameraStyle.Combat);
    //    }
    //}

    private void HandleHostRotation()
    {
        Vector3 viewDir = hostObj.position - transform.position;
        viewDir.y = 0f;
        Orientation.forward = viewDir.normalized;

        if (currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = Orientation.forward * verticalInput + Orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
            {
                hostObj.forward = Vector3.Slerp(hostObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = CombatLookAt.position - transform.position;
            dirToCombatLookAt.y = 0f;
            Orientation.forward = dirToCombatLookAt.normalized;
            hostObj.forward = dirToCombatLookAt.normalized;
        }
    }

    //private void SwitchCameraStyle(CameraStyle newStyle)
    //{
    //    CombatCam.Priority = 0;
    //    BasicCam.Priority = 0;

    //    if (newStyle == CameraStyle.Basic) BasicCam.Priority = 10;
    //    if (newStyle == CameraStyle.Combat) CombatCam.Priority = 10;

    //    currentStyle = newStyle;
    //}

    private void ResetHostVariables()
    {
        Orientation = null;
        CombatLookAt = null;
        hostObj = null;
        cc = null;
    }
}
