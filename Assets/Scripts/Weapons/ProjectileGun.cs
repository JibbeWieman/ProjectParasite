using UnityEngine;
using TMPro;

//public enum WeaponShootType
//{
//    Manual,
//    Automatic,
//    Charge,
//}

//[System.Serializable]
//public struct CrosshairData
//{
//    [Tooltip("The image that will be used for this weapon's crosshair")]
//    public Sprite CrosshairSprite;

//    [Tooltip("The size of the crosshair image")]
//    public int CrosshairSize;

//    [Tooltip("The color of the crosshair image")]
//    public Color CrosshairColor;
//}

public class ProjectileGun : MonoBehaviour
{
    #region VARIABLES
    //Bullet 
    [SerializeField] private GameObject bullet;
    [SerializeField] private Rigidbody gunRb;
    private float m_LastTimeShot = Mathf.NegativeInfinity;

    [SerializeField] private float shootForce, upwardForce; //bullet force

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShooting, BulletSpreadAngle, reloadTime, DelayBetweenShots;
    [SerializeField] private int magazineSize, bulletsPerTap;
    [SerializeField] private bool allowButtonHold;

    [Tooltip("The type of weapon wil affect how it shoots")]
    public WeaponShootType ShootType;

    private int m_CurrentAmmo, m_AmmoFired;

    [Header("Recoil")]
    [SerializeField] private float recoilForce;

    [Header("Bools")]
    [SerializeField]
    private bool hasSpread;
    private bool shooting, readyToShoot, reloading;
    private bool m_FlashLightOn = false;
    bool m_WantsToShoot = false;

    [Header("References")]
    private Camera cam;
    [SerializeField] private Transform attackPoint;

    [Header("Graphics")]
    [SerializeField] private GameObject muzzleFlash, bulletHoleGraphic, flashLight;
    [SerializeField] private TextMeshProUGUI ammoDisplay; // should be done in weaponsmanager

    //bug fixing :D
    public bool allowInvoke = true;
    #endregion

    private void Start()
    {
        cam = Camera.main;
        ammoDisplay = GameObject.FindWithTag("AmmoCount")?.GetComponent<TextMeshProUGUI>();

        //Make sure magazine is full
        m_CurrentAmmo = magazineSize;
        readyToShoot = true;

        flashLight = gameObject.GetComponentInChildren<Light>().gameObject;
    }

    private void Update()
    {
        //Set ammo display, if it exists
        if (ammoDisplay != null)
        {
            ammoDisplay.SetText(m_CurrentAmmo / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
        }

        MyInput();
    }

    private void MyInput()
    {
        //Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && m_CurrentAmmo < magazineSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && m_CurrentAmmo <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && m_CurrentAmmo > 0)
        {
            //Set bullets shot to 0
            m_AmmoFired = bulletsPerTap;

            HandleShoot();
        }

        if (Input.GetKeyDown(KeyCode.L)) { flashLight.gameObject.SetActive(!m_FlashLightOn); }
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        if (Input.GetKeyDown(KeyCode.R) && m_CurrentAmmo < magazineSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && m_CurrentAmmo <= 0) Reload();               //Reload automatically when trying to shoot without ammo

        m_WantsToShoot = inputDown || inputHeld;
        switch (ShootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }

                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }

                return false;

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    //TryBeginCharge();
                }

                // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                //if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                //{
                //    return TryReleaseCharge();
                //}

                return false;

            default:
                return false;
        }
    }

    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1;

            return true;
        }

        return false;
    }

    #region GUN FUNCTIONS
    private void HandleShoot()
    {
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

        Vector3 bulletDirection = GetShotDirection();

        //Calculate direction from attackPoint to targetPoint
        //Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        ////Calculate spread
        //float x = Random.Range(-bulletSpread, bulletSpread);
        //float y = Random.Range(-bulletSpread, bulletSpread);

        ////Calculate Direction with Spread
        //Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0); //Just add spread to last direction

        //Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity); //Store instantiated bullet
        // Set the shooter in the bullet script
        //currentBullet.GetComponent<CustomBullet>().shooter = shooter;

        //Pull bullet from object pool
        //GameObject currentBullet = ObjectPooler.Instance.SpawnFromPool(bulletTag, attackPoint.position, Quaternion.identity);
        //if (currentBullet != null)
        //{

        //Rotate bullet to shoot direction
        currentBullet.transform.forward = bulletDirection.normalized;

        //Add forces to bullet 
        currentBullet.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(cam.transform.up * upwardForce, ForceMode.Impulse);

        //Instantiate muzzle flash
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        m_CurrentAmmo--;
        m_AmmoFired++;

        //Invoke resetShot function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            //Add recoil to player
            //HostThirdPersonCam.hostRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        //if more than one bulletsPerTap make sure to repeat shoot function
        if (m_AmmoFired < bulletsPerTap && m_CurrentAmmo > 0)
            Invoke("Shoot", DelayBetweenShots);
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
        m_CurrentAmmo = magazineSize;
        reloading = false;
    }

    Vector3 GetShotDirection()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found. Using WeaponMuzzle direction.");
            return attackPoint.forward;
        }

        Vector3 shotDirection = cam.transform.forward;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of screen

        // Layer mask to ignore AI layer
        int layerMask = ~LayerMask.GetMask("AI");

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            shotDirection = (hit.point - attackPoint.position).normalized;
        }

        if (hasSpread)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadDirection = Vector3.Slerp(shotDirection, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }

        return shotDirection;
    }
    #endregion
}