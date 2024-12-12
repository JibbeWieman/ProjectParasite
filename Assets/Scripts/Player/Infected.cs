using Cinemachine;
using System.Collections;
using UnityEngine;

public class Infected : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    public GameObject parasite;

    public CinemachineFreeLook HostBasicCam;
    public CinemachineFreeLook HostCombatCam;

    //private const float maxPossesionTime = 30f;

    [Header("Keybinds")]
    private KeyCode exitHost = KeyCode.F;
    #endregion

    #region CAMERA MANAGEMENT
    private void Awake()
    {
        CinemachineFreeLook[] freeLookCams = GetComponentsInChildren<CinemachineFreeLook>();
        foreach (CinemachineFreeLook cam in freeLookCams)
        {
            if (cam.gameObject.name == "Host_BasicCam")
                HostBasicCam = cam;
            else if (cam.gameObject.name == "Host_CombatCam")
                HostCombatCam = cam;
        }
    }

    private void OnEnable()
    {
        CameraSwitcher.Register(HostBasicCam);
        CameraSwitcher.Register(HostCombatCam);
        //CameraSwitcher.SwitchCamera(HostBasicCam); //Line for hard setting a certain camera
    }

    private void OnDisable()
    {
        CameraSwitcher.Unregister(HostBasicCam);
        CameraSwitcher.Unregister(HostCombatCam);
    }
    #endregion

    #region MAX HOST POSSESION TIMER
    //Can also do it by decreasing the hosts health by 1 each second
    /*private void Start()
    {
        StartCoroutine(HostTimer());
    }
    IEnumerator HostTimer()
    {
        float timeRemaining = maxPossesionTime;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            yield return null; // Wait until the next frame
        }

        if (timeRemaining <= 0)
        {
            LeaveHost();
        }
    }*/
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(exitHost) && InfectAbility.inHost)
            LeaveHost();

        // Bug fix for when host dies while inHost :D
        parasite.transform.position = transform.position;
        parasite.transform.rotation = transform.rotation;
    }

    private void LeaveHost()
    {
        // Set parasite to the host's position and rotation
        parasite.transform.position = transform.position;
        parasite.transform.rotation = transform.rotation;

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
        parasite.SetActive(true);
        parasite.GetComponent<PlayerMovement>().enabled = true;

        this.transform.SetParent(GameObject.FindWithTag("HostRefugeCamp").transform);

        Infected scriptToRemove = GetComponent<Infected>();
        if (scriptToRemove != null)
        {
            Destroy(scriptToRemove);
        }

        InfectAbility.inHost = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Item"))
        {

            Destroy(collider.gameObject);

        }
    }

    private void CopyScriptProperties(MonoBehaviour sourceScript, MonoBehaviour targetScript)
    {
        // Use reflection to copy serialized fields
        var sourceFields = sourceScript.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var targetFields = targetScript.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var sourceField in sourceFields)
        {
            var targetField = System.Array.Find(targetFields, f => f.Name == sourceField.Name);
            if (targetField != null && targetField.FieldType == sourceField.FieldType)
            {
                // Set the value only if the field is writable and types match
                if (targetField.IsPublic || targetField.IsFamily || targetField.IsAssembly)
                {
                    targetField.SetValue(targetScript, sourceField.GetValue(sourceScript));
                }
            }
        }
    }
}




/*
using Cinemachine;
using System.Collections;
using UnityEngine;

public class Infected : MonoBehaviour
{
    [Header("References")]
    public GameObject parasite;
    public GameObject host;

    public GameObject parasiteCam;
    public GameObject hostCam;
    public CinemachineFreeLook m_hostCam;

    //[SerializeField] private Transform focusObjectTransform;
    //[SerializeField] private Transform focusObjectOrientation;

    public bool isLeeching = false;
    static public bool inHost = false;

    [Header("Keybinds")]
    private KeyCode leech = KeyCode.Space;
    private KeyCode exitHost = KeyCode.F;

    private void Awake()
    {
        Camera.main.gameObject.TryGetComponent<CinemachineBrain>(out var brain);
        if (brain == null)
        {
            brain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            Debug.Log(inHost);
        }

        // Check and update the inHost status based on the current GameObject's tag
        //inHost = CheckIfInHost();

        //parasiteCam.m_Follow = focusObjectTransform;
        //parasiteCam.m_LookAt = focusObjectOrientation;

        if (Input.GetKeyDown(leech) && !isLeeching && !inHost)
            StartCoroutine(Leeching());

        else if (Input.GetKeyDown(exitHost) && inHost)
            LeaveHost();

        //if (parCollider.bounds.Intersects(hostCollider.bounds))
        //{
        //    EnterHost(collision.gameObject);
        //}
    }

    private void EnterHost(GameObject hostObject)
    {
        isLeeching = false;
        inHost = true;

        parasite.SetActive(false);
        parasiteCam.SetActive(false);
        hostCam.SetActive(true);

        host = hostObject; // Update the host reference to the correct host object

        host.GetComponent<PlayerMovement>().enabled = true;
        parasite.GetComponent<PlayerMovement>().enabled = false;
        //host.GetComponentInChildren<ThirdPersonCam>().enabled = true;
        //parasite.GetComponentInChildren<ThirdPersonCam>().enabled = false;

        Infected newScript = host.GetComponent<Infected>();
        if (newScript == null)
        {
            newScript = host.AddComponent<Infected>();
        }

        CopyScriptProperties(this, newScript);

        Debug.Log("Entering host.");
    }

    private void LeaveHost()
    {
        // Set parasite to the host's position and rotation
        parasite.transform.position = host.transform.position;
        parasite.transform.rotation = host.transform.rotation;

        // Switch cameras and activate/deactivate the appropriate objects
        parasite.SetActive(true);
        parasiteCam.SetActive(true);
        hostCam.SetActive(false);

        host.GetComponent<PlayerMovement>().enabled = false;
        parasite.GetComponent<PlayerMovement>().enabled = true;
        //host.GetComponentInChildren<ThirdPersonCam>().enabled = false;
        //parasite.GetComponentInChildren<ThirdPersonCam>().enabled = true;

        Infected scriptToRemove = host.GetComponent<Infected>();
        if (scriptToRemove != null)
        {
            Destroy(scriptToRemove);
        }

        // Set inHost to false as we've left the host
        inHost = false;
    }

    private bool CheckIfInHost()
    {
        return gameObject.CompareTag("Host");
    }

    IEnumerator Leeching()
    {
        isLeeching = true;
        Debug.Log("Leeching started");

        yield return new WaitForSeconds(2);

        isLeeching = false;
        Debug.Log("Leeching ended");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Host") && isLeeching)
        {

            EnterHost(collision.gameObject);

        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Item"))
        {

            Destroy(collider.gameObject);

        }
    }

    private void CopyScriptProperties(MonoBehaviour sourceScript, MonoBehaviour targetScript)
    {
        // Use reflection to copy serialized fields
        var sourceFields = sourceScript.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var targetFields = targetScript.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var sourceField in sourceFields)
        {
            var targetField = System.Array.Find(targetFields, f => f.Name == sourceField.Name);
            if (targetField != null && targetField.FieldType == sourceField.FieldType)
            {
                // Set the value only if the field is writable and types match
                if (targetField.IsPublic || targetField.IsFamily || targetField.IsAssembly)
                {
                    targetField.SetValue(targetScript, sourceField.GetValue(sourceScript));
                }
            }
        }
    }

    public void SwitchCameraTarget(Transform newTarget)
    {
        m_hostCam.Follow = newTarget;
        m_hostCam.LookAt = newTarget;
    }

}
*/