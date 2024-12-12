//using UnityEngine;
//using TMPro;

//interface IInteractable
//{
//    void Interact();
//    string GetName(); // Add a method to get the name of the object
//}

//public class Interactor : MonoBehaviour
//{
//    public Transform InteractorSource;
//    public float interactRange;
//    public GameObject interactText; // Assign this in the inspector
//    public TextMeshProUGUI interactTextText; // Assign this in the inspector
//    public Camera cam; // Assign the Cinemachine camera in the inspector

//    //private void Start()
//    //{
//    //    Collider[] childColliders = InteractorSource.GetComponentsInChildren<Collider>();
//    //    foreach (Collider childCollider in childColliders)
//    //    {
//    //        Physics.IgnoreCollision(InteractorSource.GetComponent<Collider>(), childCollider, true);
//    //    }
//    //}

//    private void Update()
//    {
//        if (cam == null) cam = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
//        if (interactText == null) interactText = GameObject.FindWithTag("InteractText");
//        if (interactTextText == null) interactTextText = interactText?.GetComponent<TextMeshProUGUI>();

//        // Convert the screen center to world space using the defined distance
//        Vector3 screenCenter = new Vector3(0.5f, 0.5f, cam.nearClipPlane);

//        // Convert the screen center to world space
//        Vector3 worldPosition = cam.ViewportToWorldPoint(screenCenter);

//        Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
//        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactRange))
//        {
//            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
//            {
//                // Display the name of the interactable object
//                interactTextText.text = interactObj.GetName();
//                interactText.SetActive(true);
//                PositionText(worldPosition);
                
//                if (Input.GetKeyDown(KeyCode.E))
//                {
//                    interactObj.Interact();
//                }
//            }
//            else
//            {
//                interactText.SetActive(false);
//            }
//        }
//        else
//        {
//            interactText.SetActive(false);
//        }
//    }

//    private void PositionText(Vector3 position)
//    {
//        // Set the position of the text object
//        interactText.transform.position = position;

//        // Make the text face the camera
//        interactText.transform.LookAt(cam.transform);
//        interactText.transform.Rotate(0, 180, 0); // Reverse the rotation so it faces the camera
//    }


//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.white;
//        Gizmos.DrawWireSphere(transform.position, interactRange);
//    }
//}