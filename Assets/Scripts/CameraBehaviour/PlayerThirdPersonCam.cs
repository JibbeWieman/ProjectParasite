using UnityEngine;

public class PlayerThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private GameObject basicCam;


    private void Update()
    {
        if (Events.ActorPossesedEvent.InHost == false)
        {
            //rotate orientation
            Vector3 viewDir = player.position - transform.position;
            viewDir.y = 0f;
            orientation.forward = viewDir.normalized;

            //rotate player object
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
