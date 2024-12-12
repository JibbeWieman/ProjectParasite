using UnityEngine;
using Cinemachine;

public class HostThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public static Transform hostOrientation;
    public static Transform hostCombatLookAt;
    public static Transform hostObj;
    public static Rigidbody hostRb;

    public static CinemachineFreeLook hostBasicCam;
    public static CinemachineFreeLook hostCombatCam;

    public float rotationSpeed;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat
    }

    private void Update()
    {
        if (InfectAbility.inHost == true && InfectAbility.host != null)
        {
            SetHostVariables();

            //switch styles
            if (!InfectAbility.host.GetComponentInChildren<ProjectileGun>()
                || (InfectAbility.host.GetComponentInChildren<ProjectileGun>() && Input.GetKeyUp(KeyCode.Mouse1)))
            {
                SwitchCameraStyle(CameraStyle.Basic);
            }
            if (InfectAbility.host.GetComponentInChildren<ProjectileGun>() && Input.GetKeyDown(KeyCode.Mouse1))
            {
                SwitchCameraStyle(CameraStyle.Combat);
            }

            //rotate orientation
            Vector3 viewDir = InfectAbility.host.transform.position - transform.position;
            viewDir.y = 0f;
            hostOrientation.forward = viewDir.normalized;

            //rotate player object
            if (currentStyle == CameraStyle.Basic)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDir = hostOrientation.forward * verticalInput + hostOrientation.right * horizontalInput;

                if (inputDir != Vector3.zero)
                {
                    hostObj.forward = Vector3.Slerp(hostObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
                }
            }

            else if (currentStyle == CameraStyle.Combat)
            {
                Vector3 dirToCombatLookAt = hostCombatLookAt.position - transform.position;
                dirToCombatLookAt.y = 0f;
                hostOrientation.forward = dirToCombatLookAt.normalized;

                hostObj.forward = dirToCombatLookAt.normalized;
            }
        }
        else
        {
            //Set variables to null so the next time you enterHost you get the right variables
            if (hostBasicCam != null || hostCombatCam != null)
            {
                hostCombatCam.Priority = 0;
                hostBasicCam.Priority = 0;
            }

            hostOrientation = null;
            hostCombatLookAt = null;
            hostObj = null;
            hostRb = null;
            hostBasicCam = null;
            hostCombatCam = null;
        }
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        hostCombatCam.Priority = 0;
        hostBasicCam.Priority = 0;

        if (newStyle == CameraStyle.Basic) hostBasicCam.Priority = 10;
        if (newStyle == CameraStyle.Combat) hostCombatCam.Priority = 10;

        currentStyle = newStyle;
    }

    private void SetHostVariables()
    {
        //Set hostObj transform
        if (hostObj == null)
            hostObj = InfectAbility.host.transform.Find("Body");

        //Set rigidbody
        if (hostRb == null)
            hostRb = InfectAbility.host.GetComponent<Rigidbody>();

        //Set orientation and combatLookAt
        if (hostOrientation == null)
            hostOrientation = InfectAbility.host.transform.Find("Orientation");

        if (hostCombatLookAt == null)
            hostCombatLookAt = hostOrientation.transform.Find("CombatLookAt");

        //Set camera variables
        if (hostBasicCam == null)
            hostBasicCam = InfectAbility.host.transform.Find("Host_BasicCam").GetComponent<CinemachineFreeLook>();

        if (hostCombatCam == null)
            hostCombatCam = InfectAbility.host.transform.Find("Host_CombatCam").GetComponent<CinemachineFreeLook>(); ;
    }
}
