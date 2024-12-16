using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public ProjectileGun gunScript;
    public Rigidbody rb;
    public BoxCollider coll;
    public GameObject player = null;
    public Transform gunContainer;
    public Transform orientation = null; 
    //removed fpsCam variable which was used for throwing and dropping in the cam direction

    public float pickUpRange;
    public float dropForwardForce, dropUpwardsForce;

    public bool equipped;
    public static bool slotFull;

    public string objectName = "Gun";

    private void Start()
    {
        //Setup
        if (!equipped)
        {
            gunScript.enabled = false;
            rb.isKinematic = false;
            coll.isTrigger = false;
        }
        if (equipped)
        {
            gunScript.enabled = true;
            rb.isKinematic = true;
            coll.isTrigger = true;
            slotFull = true;
        }
    }

    private void Update()
    {
        if (GetComponentInParent<EnemyAI>().m_IsDead)
        {
            Drop();
        }

        if (GetComponentInParent<Infected>() != null)
        {
            //Set player to whoever host the parasite is controlling
            if (player == null)
                player = InfectAbility.host;
            //Set orientation to that of the host
           // if (orientation == null)
                //orientation = HostThirdPersonCam.hostCombatLookAt;

            //Check if player is in range and "E" is pressed
            Vector3 distanceToPlayer = player.transform.position - transform.position;
            if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.E) && !slotFull) PickUp();

            //Drop if equipped and "Q" is pressed
            if (equipped && Input.GetKeyDown(KeyCode.Q)) Drop();
        }
        else
        {
            if (player != null || orientation != null) // Reset only when exiting host
            {
                player = null;
                orientation = null;

                Debug.Log("Reset player and orientation to null");
            }
        }
    }

    private void PickUp()
    {
        equipped = true;
        slotFull = true;

        //Make weapon a child of the camera and move it to default position
        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        //Make Rigidbody kinematic and BoxCollider a trigger
        rb.isKinematic = true;
        coll.isTrigger = true;

        //Enable script
        gunScript.enabled = true;
    }

    private void Drop()
    {
        equipped = false;
        slotFull = false;

        //Set parent to null
        transform.SetParent(null);

        //Make Rigidbody not kinematic and BoxCollider normal
        rb.isKinematic = false;
        coll.isTrigger = false;

        //Gun carries momentum of player
        rb.velocity = player.GetComponent<Rigidbody>().velocity;

        //AddForce
        rb.AddForce(orientation.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(orientation.up * dropUpwardsForce, ForceMode.Impulse);
        //Add random rotation
        float random = Random.Range(-1, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10);

        //Disable script
        gunScript.enabled = false;
    }
}
