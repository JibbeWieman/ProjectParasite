using System.Collections;
using UnityEngine;
using Cinemachine;

public class InfectAbility : AbilityBase
{
    #region VARIABLES
    [Header("References")]
    public GameObject parasite;
    public static GameObject host;
    public GameObject persistentHosts;

    public CinemachineFreeLook ParasiteBasicCam;

    public bool isLeeching = false;
    public static bool inHost = false;

    public ParticleSystem blood;

    [Header("Keybinds")]
    private KeyCode key = KeyCode.Space;
    #endregion

    #region CAMERA MANAGEMENT
    private void OnEnable()
    {
        CameraSwitcher.Register(ParasiteBasicCam);
        //CameraSwitcher.SwitchCamera(ParasiteBasicCam); Line for hard setting a certain camera
    }

    private void OnDisable()
    {
        CameraSwitcher.Unregister(ParasiteBasicCam);
    }
    #endregion 

    private void Update()
    {
        if (Input.GetKeyDown(key) && !isLeeching && !inHost && canUse)
            StartCoroutine(Leeching());
    }

    public override void Ability()
    {
        isLeeching = false;
        inHost = true;

        Infected newScript = host.GetComponent<Infected>();
        if (newScript == null)
        {
            newScript = host.AddComponent<Infected>();
        }
        CopyScriptProperties(this, newScript);

        //Make the host persistent
        host.transform.SetParent(persistentHosts.transform);

        var hostAI = host.GetComponent<EnemyAI>();
        var hostMovement = host.GetComponent<PlayerMovement>();
        var hostRb = host.GetComponent<Rigidbody>();

        if (!hostAI.m_IsDead)
        {
            hostMovement.enabled = true;
            hostRb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            hostRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        parasite.SetActive(false);
        parasite.GetComponent<PlayerMovement>().enabled = false;

        CameraSwitcher.SwitchCamera(host.GetComponent<Infected>().HostBasicCam);

        Debug.Log("Entering host.");
    } 
    IEnumerator Leeching()
    {
        isLeeching = true;
        Debug.Log("Leeching started");

        yield return new WaitForSeconds(2f);

        isLeeching = false;
        Debug.Log("Leeching ended");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Host") && isLeeching)
        {
            //Blood particle effect
            ParticleSystem instantiatedBlood = Instantiate(blood, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            instantiatedBlood.transform.SetParent(collision.gameObject.transform);
            Destroy(instantiatedBlood, 1);

            host = collision.gameObject; // Update the host reference to the correct host object

            TriggerAbility();
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