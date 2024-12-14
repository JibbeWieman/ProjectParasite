using UnityEngine;
using TMPro;

public class ProjectileGun : MonoBehaviour
{
    #region VARIABLES
    public GameObject shooter; //Reference to the shooter of the gun

    //Bullet 
    public GameObject bullet;
    public Rigidbody gunRb;

    //bullet force
    public float shootForce, upwardForce;

    //Bullet tag
    //private string bulletTag = "Bullet";

    [Header("Gun stats")]
    //
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    [Header("Recoil")]
    //public Rigidbody playerRb;
    public float recoilForce;

    [Header("Bools")]
    //bools
    bool shooting, readyToShoot, reloading;

    [Header("References")]
    public Camera cam;
    public Transform attackPoint;

    [Header("Graphics")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public GameObject flashLight;
    private bool m_FlashLightOn = true;
    public TextMeshProUGUI ammoDisplay;

    //bug fixing :D
    public bool allowInvoke = true;
    #endregion

    private void Awake()
    {
        //Make sure magazine is full
        bulletsLeft = magazineSize;
        readyToShoot = true;

        flashLight = gameObject.GetComponentInChildren<Light>().gameObject;
    }

    private void Update()
    {
        #region ASSIGN VARIABLES
        if (cam == null)
        {
            cam = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        }

        if (ammoDisplay == null)
        {
            ammoDisplay = GameObject.FindWithTag("AmmoCount")?.GetComponent<TextMeshProUGUI>();
        }
        #endregion

        if (gameObject.GetComponentsInParent<Infected>().Length > 0)
        {
            MyInput();

            //Set ammo display, if it exists
            if (ammoDisplay != null)
            {
                ammoDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
            }
        }

        //Flashlight activation
        if (m_FlashLightOn) { flashLight.gameObject.SetActive(false); }
        else { flashLight.gameObject.SetActive(true); }
    }

    private void MyInput()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            m_FlashLightOn = !m_FlashLightOn;
        }

        //Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        //Reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        //Reload automatically when trying to shoot without ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            //Set bullets shot to 0
            bulletsShot = bulletsPerTap;

            Shoot();
        }
    }

    #region GUN FUNCTIONS
    private void Shoot()
    {
        shooter = gameObject.transform.root.GetChild(0).gameObject; //Get the second highest gameobject from the hierachy
        Debug.Log(shooter);

        //Debug.Log("I'm Shooting!");
        readyToShoot = false;

        //Find the exact hit position using a raycast
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Just a ray through the middle of your screen
        RaycastHit hit;

        //check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75); //Just a point far way from the player

        //Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculate Direction with Spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0); //Just add spread to last direction

        //Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity); //Store instantiated bullet
        // Set the shooter in the bullet script
        //currentBullet.GetComponent<CustomBullet>().shooter = shooter;

        //Pull bullet from object pool
        //GameObject currentBullet = ObjectPooler.Instance.SpawnFromPool(bulletTag, attackPoint.position, Quaternion.identity);
        //if (currentBullet != null)
        //{

        //Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        //Add forces to bullet 
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(cam.transform.up * upwardForce, ForceMode.Impulse);

        //Instantiate muzzle flash
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        //Invoke resetShot function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            //Add recoil to player
            HostThirdPersonCam.hostRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
        //}
    }
        private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
    #endregion
}