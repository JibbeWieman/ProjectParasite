using UnityEngine;

public class PlayerThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    //public Transform combatLookAt;

    public GameObject basicCam;
    //public GameObject combatCam;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        //Combat
    }

    private void Update()
    {
        if (InfectAbility.inHost == false)
        {
            //switch styles
            //if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
            //if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Combat);

            //rotate orientation
            Vector3 viewDir = player.position - transform.position;
            viewDir.y = 0f;
            orientation.forward = viewDir.normalized;

            //rotate player object
            if (currentStyle == CameraStyle.Basic)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

                if (inputDir != Vector3.zero)
                {
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
                }
            }

            /* else if (currentStyle == CameraStyle.Combat)
            {
                Vector3 dirToCombatLookAt = combatLookAt.position - transform.position;
                dirToCombatLookAt.y = 0f;
                orientation.forward = dirToCombatLookAt.normalized;

                playerObj.forward = dirToCombatLookAt.normalized;
            } */
        }
    }

    /* private void SwitchCameraStyle(CameraStyle newStyle)
    {
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);

        currentStyle = newStyle;
    } */
}
