using System.Collections;
using UnityEngine;
using Cinemachine;

public class InfectAbility : AbilityBase
{
    #region VARIABLES
    [Header("References")]
    private GameObject parasite;
    public static GameObject host;
    public GameObject persistentHosts;

    public CinemachineFreeLook ParasiteBasicCam;

    public bool isLeeching = false;
    public static bool inHost = false;

    public ParticleSystem blood;
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
    private void Start()
    {
        parasite = gameObject;
    }

    private void Update()
    {
        if (Input.GetButtonDown(GameConstants.k_ButtonNameLeech))
        {
            if (!isLeeching && !inHost && canUse)
            {
                StartCoroutine(Leeching());
            }
        }
    }

    public override void Ability()
    {
        //Debug.Log($"3 Should be different: {Events.ActorPossesedEvent.CurrentActor}");
        isLeeching = false;
        inHost = true;

        //Infected newScript = host.GetComponent<Infected>();
        //if (newScript == null)
        //{
        //    newScript = host.AddComponent<Infected>();
        //}
        //CopyScriptProperties(this, newScript);

        //Make the host persistent
        host.transform.SetParent(persistentHosts.transform);

        parasite.SetActive(false);

        //CameraSwitcher.SwitchCamera(host.GetComponent<Infected>().HostBasicCam);

        Debug.Log("Entering host.");
    } 
    private IEnumerator Leeching()
    {
        isLeeching = true;
        Debug.Log("Leeching started");

        yield return new WaitForSeconds(2f);

        isLeeching = false;
        Debug.Log("Leeching ended");
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"Player hit: {hit.gameObject.name}");

        if (hit.gameObject.layer == LayerMask.NameToLayer("AI") && isLeeching)
        {
            // Play blood particle effect
            ParticleSystem instantiatedBlood = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.normal));
            instantiatedBlood.transform.SetParent(hit.gameObject.transform);
            Destroy(instantiatedBlood, 1);

            parasite.SetActive(false);
            Debug.Log("Trying to possess");
            //Debug.Log($"1: { Events.ActorPossesedEvent.CurrentActor}");
            Events.ActorPossesedEvent.CurrentActor = hit.gameObject.GetComponent<Actor>().id;

            TriggerAbility();
            //Debug.Log($"2 Trigger: {Events.ActorPossesedEvent.CurrentActor}");

            host = hit.gameObject;
        }
    }

    /*void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Player collided with: {collision.gameObject.name}");
        if (collision.gameObject.layer == LayerMask.NameToLayer("AI") && isLeeching)
        {
            //Blood particle effect
            ParticleSystem instantiatedBlood = Instantiate(blood, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            instantiatedBlood.transform.SetParent(collision.gameObject.transform);
            Destroy(instantiatedBlood, 1);

            Debug.Log("Trying to posses");
            //host = collision.gameObject; // Update the host reference to the correct host object
            Events.ActorPossesedEvent.CurrentActor = collision.gameObject.GetComponent<Actor>().id;

            TriggerAbility();
        }
    } */

    /*private void CopyScriptProperties(MonoBehaviour sourceScript, MonoBehaviour targetScript)
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
    }*/
}