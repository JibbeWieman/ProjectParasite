using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image that will be used for this weapon's crosshair")]
    public Sprite CrosshairSprite;

    [Tooltip("The size of the crosshair image")]
    public int CrosshairSize;

    [Tooltip("The color of the crosshair image")]
    public Color CrosshairColor;
}

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    #region Variables
    [Header("Information")]
    [Tooltip("The name that will be displayed in the UI for this weapon")]
    public string WeaponName;

    [Tooltip("The image that will be displayed in the UI for this weapon")]
    public Sprite WeaponIcon;

    [Tooltip("Default data for the crosshair")]
    public CrosshairData CrosshairDataDefault;

    [Tooltip("Data for the crosshair when targeting an enemy")]
    public CrosshairData CrosshairDataTargetInSight;

    [Header("Internal References")]
    [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
    public GameObject WeaponRoot;

    [Tooltip("Tip of the weapon, where the projectiles are shot")]
    public Transform WeaponMuzzle;

    [Header("Shoot Parameters")]
    [Tooltip("The type of weapon wil affect how it shoots")]
    public WeaponShootType ShootType;

    [Tooltip("The projectile prefab")] public ProjectileBase ProjectilePrefab;

    [Tooltip("Minimum duration between two shots")]
    public float DelayBetweenShots = 0.5f;

    [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    public float BulletSpreadAngle = 0f;

    [Tooltip("Amount of bullets per shot")]
    public int BulletsPerShot = 1;

    [Tooltip("Force that will push back the weapon after each shot")]
    [Range(0f, 2f)]
    public float RecoilForce = 1;

    [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
    [Range(0f, 1f)]
    public float AimZoomRatio = 1f;

    [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
    public Vector3 AimOffset;

    [Header("Ammo Parameters")]
    public bool UsesPooling = true;
    [Tooltip("Should the player manually reload")]
    public bool AutomaticReload = true;
    [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
    public bool HasPhysicalBullets = false;
    [Tooltip("Number of bullets in a mag")]
    public int MagSize = 30;
    [Tooltip("Bullet Shell Casing")]
    public GameObject ShellCasing;
    [Tooltip("Weapon Ejection Port for physical ammo")]
    public Transform EjectionPort;
    [Tooltip("Force applied on the shell")]
    //[Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
    [Range(0.0f, 100.0f)] public float ShootingForce = 40.0f;
    [Tooltip("Maximum number of shell that can be spawned before reuse")]
    [Range(1, 30)] public int ShellPoolSize = 1;
    [Tooltip("Amount of ammo reloaded per second")]
    public float AmmoReloadRate = 1f;

    [Tooltip("Delay after the last shot before starting to reload")]
    public float AmmoReloadDelay = 2f;

    [Tooltip("Maximum amount of ammo in the gun")]
    public int MaxAmmo = 8;

    [Header("Charging parameters (charging weapons only)")]
    [Tooltip("Trigger a shot when maximum charge is reached")]
    public bool AutomaticReleaseOnCharged;

    [Tooltip("Duration to reach maximum charge")]
    public float MaxChargeDuration = 2f;

    [Tooltip("Initial ammo used when starting to charge")]
    public float AmmoUsedOnStartCharge = 1f;

    [Tooltip("Additional ammo used when charge reaches its maximum")]
    public float AmmoUsageRateWhileCharging = 1f;

    [Header("Audio & Visual")]
    [Tooltip("Optional weapon animator for OnShoot animations")]
    public Animator WeaponAnimator;

    [Tooltip("Prefab of the muzzle flash")]
    public GameObject MuzzleFlashPrefab;

    [Tooltip("Unparent the muzzle flash instance on spawn")]
    public bool UnparentMuzzleFlash;

    [Tooltip("sound played when shooting")]
    public AudioClip[] ShootSfx;
    public AudioClip ReloadSfx;

    [Tooltip("Sound played when changing to this weapon")]
    public AudioClip ChangeWeaponSfx;

    [Tooltip("Continuous Shooting Sound")] public bool UseContinuousShootSound = false;
    public AudioClip ContinuousShootStartSfx;
    public AudioClip ContinuousShootLoopSfx;
    public AudioClip ContinuousShootEndSfx;
    AudioSource m_ContinuousShootAudioSource = null;
    bool m_WantsToShoot = false;

    public UnityAction OnShoot;
    //public event Action OnShootProcessed;

    int m_CarriedPhysicalBullets;
    float m_CurrentAmmo;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    public float LastChargeTriggerTimestamp { get; private set; }
    Vector3 m_LastMuzzlePosition;

    public GameObject Owner { get; set; }
    public GameObject SourcePrefab { get; set; }
    public bool IsCharging { get; private set; }
    public float CurrentAmmoRatio { get; private set; }
    public bool IsWeaponActive { get; private set; }
    public bool IsCooling { get; private set; }
    public float CurrentCharge { get; private set; }
    public Vector3 MuzzleWorldVelocity { get; private set; }

    public float GetAmmoNeededToShoot() =>
        (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
        (MaxAmmo * BulletsPerShot);

    public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
    public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

    AudioSource m_ShootAudioSource;

    public bool IsReloading { get; private set; }

    const string k_AnimAttackParameter = "Attack";

    private Queue<ProjectileBase> m_PhysicalAmmoPool;
    private Queue<ProjectileBase> m_ActiveAmmoPool;
    #endregion

    void Start()
    {
        m_CurrentAmmo = MaxAmmo;
        m_CarriedPhysicalBullets = HasPhysicalBullets ? MagSize : 0;
        m_LastMuzzlePosition = WeaponMuzzle.position;

        m_ShootAudioSource = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this, gameObject);

        if (UseContinuousShootSound)
        {
            m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
            m_ContinuousShootAudioSource.playOnAwake = false;
            m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
            m_ContinuousShootAudioSource.outputAudioMixerGroup =
                AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
            m_ContinuousShootAudioSource.loop = true;
        }

        if (HasPhysicalBullets)
        {
            m_PhysicalAmmoPool = new Queue<ProjectileBase>(ShellPoolSize);
            m_ActiveAmmoPool = new Queue<ProjectileBase>();

            for (int i = 0; i < MaxAmmo; i++)
            {
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab);
                newProjectile.transform.SetParent(transform.parent, false);
                newProjectile.gameObject.SetActive(false);
                m_PhysicalAmmoPool.Enqueue(newProjectile);
            }
        }
    }

    void Update()
    {
        UpdateAmmo();
        UpdateCharge();
        UpdateContinuousShootSound();

        if (Time.deltaTime > 0)
        {
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }
    }

    public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

    #region Shooting Logic
    public bool TryShoot(GameObject target = null)
    {
        if (IsReloading || m_CurrentAmmo < 1f || m_LastTimeShot + DelayBetweenShots > Time.time)
            return false;

        Shoot(target);
        UseAmmo(1f);
        return true;
    }

    private void Shoot(GameObject target = null)
    {
        Debug.Log($"Shooting at: {target}");
        Vector3 shotDirection = GetShotDirection(target);

        ProjectileBase newProjectile = UsesPooling ? GetPooledProjectile() : InstantiateProjectile();
        newProjectile.transform.SetPositionAndRotation(WeaponMuzzle.position, Quaternion.LookRotation(shotDirection));
        newProjectile.gameObject.SetActive(true);

        Rigidbody projectileRb = newProjectile.GetComponent<Rigidbody>();
        projectileRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        projectileRb.AddForce(shotDirection * ShootingForce, ForceMode.VelocityChange);
        newProjectile.Shoot(this);

        HandleMuzzleFlash();
        PlayShootEffects();
        m_LastTimeShot = Time.time;
    }

    private ProjectileBase GetPooledProjectile()
    {
        if (m_PhysicalAmmoPool.Count == 0) return null;
        ProjectileBase proj = m_PhysicalAmmoPool.Dequeue();
        proj.transform.SetParent(null);
        return proj;
    }

    private ProjectileBase InstantiateProjectile()
    {
        return Instantiate(ProjectilePrefab, WeaponMuzzle.position, Quaternion.identity);
    }

    private void HandleMuzzleFlash()
    {
        if (MuzzleFlashPrefab == null) return;
        GameObject muzzleFlash = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position, WeaponMuzzle.rotation, WeaponMuzzle.transform);
        if (UnparentMuzzleFlash) muzzleFlash.transform.SetParent(null);
        Destroy(muzzleFlash, 2f);
    }

    private void PlayShootEffects()
    {
        if (ShootSfx.Length > 0 && !UseContinuousShootSound)
            Game_Manager.PlayRandomSfx(m_ShootAudioSource, ShootSfx);

        WeaponAnimator?.SetTrigger(k_AnimAttackParameter);
    }

    public Vector3 GetShotDirection(GameObject target = null)
    {
        if (target != null)
            return (target.transform.position - WeaponMuzzle.position).normalized;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found. Using WeaponMuzzle direction.");
            return WeaponMuzzle.forward;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~LayerMask.GetMask("AI"))) //~ = tilde: Invert bitmask, so in this case it hits all layers except the AI layer
            return (hit.point - WeaponMuzzle.position).normalized;

        return WeaponMuzzle.forward;
    }
    #endregion

    void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);

    #region Reloading Logic
    private void UpdateAmmo()
    {
        if (!IsReloading && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo)
        {
            float ammoToAdd = AmmoReloadRate * Time.deltaTime;
            int intAmmoToAdd = Mathf.FloorToInt(ammoToAdd);
            float ammoNeeded = MagSize - m_CurrentAmmo;
            float ammoToLoad = Mathf.Min(intAmmoToAdd, ammoNeeded, MagSize);

            m_CurrentAmmo += ammoToLoad;
            MagSize -= Mathf.FloorToInt(ammoToLoad);
        }

        if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo && !IsCharging)
        {
            // reloads weapon over time
            m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;

            // limits ammo to max value
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);

            IsCooling = true;
        }
        else
        {
            IsCooling = false;
        }

        if (MaxAmmo == Mathf.Infinity)
        {
            CurrentAmmoRatio = 1f;
        }
        else
        {
            CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
        }
    }

    public void Reload()
    {
        if (IsReloading)
            return;

        Debug.Log("Trying to reload");
        if (m_PhysicalAmmoPool.Count < MaxAmmo || m_CurrentAmmo < MaxAmmo)
        {
            Debug.Log("Reloading");
            IsReloading = true;

            m_ShootAudioSource.PlayOneShot(ReloadSfx);

            if (UsesPooling)
            {
                // Return active projectiles to pool
                while (m_ActiveAmmoPool.Count > 0 && UsesPooling)
                {
                    ProjectileBase projectile = m_ActiveAmmoPool.Dequeue();
                    projectile.gameObject.SetActive(false);
                    m_PhysicalAmmoPool.Enqueue(projectile);
                }

                // Restore ammo from physical bullets
                m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, MagSize);
            }
            else
            {
                m_CurrentAmmo = MaxAmmo;
            }

            IsReloading = false;
        }
    }

    public void TryReload()
    {
        if (IsReloading || m_CurrentAmmo == MagSize || m_CurrentAmmo <= 0)
        {
            return;
        }

        IsReloading = true;
        Invoke(nameof(StartReloading), AmmoReloadDelay);
    }

    private void StartReloading()
    {
        IsReloading = false;
    }
    #endregion

    public void StartReloadAnimation()
    {
        //if (m_CurrentAmmo < m_CarriedPhysicalBullets)
        //{
        //    GetComponent<Animator>().SetTrigger("Reload");
        //    IsReloading = true;
        //}
    }

    void UpdateCharge()
    {
        if (IsCharging)
        {
            if (CurrentCharge < 1f)
            {
                float chargeLeft = 1f - CurrentCharge;

                // Calculate how much charge ratio to add this frame
                float chargeAdded = 0f;
                if (MaxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }
                else
                {
                    chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                }

                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // See if we can actually add this charge
                float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo based on charge added
                    UseAmmo(ammoThisChargeWouldRequire);

                    // set current charge ratio
                    CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                }
            }
        }
    }

    void UpdateContinuousShootSound()
    {
        if (UseContinuousShootSound)
        {
            if (m_WantsToShoot && m_CurrentAmmo >= 1f)
            {
                if (!m_ContinuousShootAudioSource.isPlaying)
                {
                    Game_Manager.PlayRandomSfx(m_ShootAudioSource, ShootSfx);
                    m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                    m_ContinuousShootAudioSource.Play();
                }
            }
            else if (m_ContinuousShootAudioSource.isPlaying)
            {
                m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                m_ContinuousShootAudioSource.Stop();
            }
        }
    }

    public void ShowWeapon(bool show)
    {
        WeaponRoot.SetActive(show);

        if (show && ChangeWeaponSfx)
        {
            m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
        }

        IsWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
        m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
        m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
        m_LastTimeShot = Time.time;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
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
                    TryBeginCharge();
                }

                // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                {
                    return TryReleaseCharge();
                }

                return false;

            default:
                return false;
        }
    }

    bool TryBeginCharge()
    {
        if (!IsCharging
            && m_CurrentAmmo >= AmmoUsedOnStartCharge
            && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            UseAmmo(AmmoUsedOnStartCharge);

            LastChargeTriggerTimestamp = Time.time;
            IsCharging = true;

            return true;
        }

        return false;
    }

    bool TryReleaseCharge()
    {
        if (IsCharging)
        {
            TryShoot();

            CurrentCharge = 0f;
            IsCharging = false;

            return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (WeaponMuzzle == null || Camera.main == null)
            return;

        Vector3 shotDirection = GetShotDirection();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(WeaponMuzzle.position, WeaponMuzzle.position + shotDirection * 10f);
    }
}

//OLD CODE
/* bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            Shoot();
            m_CurrentAmmo -= 1f;

            return true;
        }

        return false;
    } */
/* void Shoot()
    {
        if (m_PhysicalAmmoPool.Count > 0 || m_CurrentAmmo > 0)
        {
            if (UsesPooling)
            {
                ProjectileBase nextShell = m_PhysicalAmmoPool.Dequeue();
                m_ActiveAmmoPool.Enqueue(nextShell);

                nextShell.transform.SetPositionAndRotation(EjectionPort.transform.position, EjectionPort.transform.rotation);
                nextShell.gameObject.SetActive(true);
                nextShell.Shoot(this);
                nextShell.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                nextShell.GetComponent<Rigidbody>().AddForce(GetShotDirection() * ShootingForce, ForceMode.VelocityChange);
            }
            else
            {
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position, Quaternion.LookRotation(GetShotDirection()));

                newProjectile.transform.SetPositionAndRotation(EjectionPort.transform.position, EjectionPort.transform.rotation);
                newProjectile.gameObject.SetActive(true);
                newProjectile.Shoot(this);
                newProjectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                newProjectile.GetComponent<Rigidbody>().AddForce(GetShotDirection() * ShootingForce, ForceMode.VelocityChange);
            }
        }
        else if (m_PhysicalAmmoPool.Count <= 0 || m_CurrentAmmo <= 0)
        {
            Reload();
        }
    } */
/* void HandleShoot()
{
    int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
        ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
        : BulletsPerShot;

    if (HasPhysicalBullets && m_CurrentAmmo > 0)
    {
        for (int i = 0; i < bulletsPerShotFinal; i++)
        {
            Vector3 shotDirection = GetShotDirection();

            ProjectileBase newProjectile;

            if (UsesPooling && m_PhysicalAmmoPool.Count > 0)
            {
                newProjectile = m_PhysicalAmmoPool.Dequeue();
                newProjectile.transform.SetParent(null);
                newProjectile.transform.position = WeaponMuzzle.position;
                newProjectile.transform.rotation = Quaternion.LookRotation(shotDirection);
                newProjectile.gameObject.SetActive(true);
            }
            else
            {
                newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position, Quaternion.LookRotation(shotDirection));
            }

            newProjectile.transform.forward = shotDirection;
            newProjectile.Shoot(this);

            TryShoot();
            m_CarriedPhysicalBullets--;
        }
        if (MuzzleFlashPrefab != null)
        {
            GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position, WeaponMuzzle.rotation, WeaponMuzzle.transform);
            if (UnparentMuzzleFlash)
            {
                muzzleFlashInstance.transform.SetParent(null);
            }
            Destroy(muzzleFlashInstance, 2f);
        }
        else
        {
            Reload();
        }
    }

    m_LastTimeShot = Time.time;

    if (ShootSfx.Length > 0 && !UseContinuousShootSound)
    {
        Game_Manager.PlayRandomSfx(m_ShootAudioSource, ShootSfx);
    }

    if (WeaponAnimator)
    {
        WeaponAnimator.SetTrigger(k_AnimAttackParameter);
    }

    OnShoot?.Invoke();
    OnShootProcessed?.Invoke();
} */
/* public void AIShoot(GameObject target)
{
    Debug.Log($"Trying to Shoot: {target}");
    Vector3 aiShotDirection = GetShotDirection(target);

    ProjectileBase newProjectile;

    if (UsesPooling)
    {
        newProjectile = m_PhysicalAmmoPool.Dequeue();
        newProjectile.transform.SetParent(null);
        newProjectile.transform.position = WeaponMuzzle.position;
        newProjectile.transform.rotation = Quaternion.LookRotation(aiShotDirection);
        newProjectile.gameObject.SetActive(true);
    }
    else
    {
        newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position, Quaternion.LookRotation(aiShotDirection));
    }

    newProjectile.Shoot(this);
    newProjectile.transform.SetPositionAndRotation(EjectionPort.transform.position, EjectionPort.transform.rotation);
    newProjectile.transform.forward = aiShotDirection;
    newProjectile.gameObject.SetActive(true);
    newProjectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
    newProjectile.GetComponent<Rigidbody>().AddForce(GetShotDirection(target) * ShootingForce, ForceMode.VelocityChange);

    if (MuzzleFlashPrefab != null)
    {
        GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position, WeaponMuzzle.rotation, WeaponMuzzle.transform);
        if (UnparentMuzzleFlash)
        {
            muzzleFlashInstance.transform.SetParent(null);
        }
        Destroy(muzzleFlashInstance, 2f);
    }
    else
    {
        Reload();
    }

    if (ShootSfx.Length > 0 && !UseContinuousShootSound)
    {
        Game_Manager.PlayRandomSfx(m_ShootAudioSource, ShootSfx);
    }

    if (WeaponAnimator)
    {
        WeaponAnimator.SetTrigger(k_AnimAttackParameter);
    }
} */