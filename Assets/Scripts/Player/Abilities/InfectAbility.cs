using System.Collections;
using UnityEngine;
using Cinemachine;

public class InfectAbility : AbilityBase
{
    #region VARIABLES
    [Header("References")]
    [SerializeField]
    ParticleSystem bloodVfx;

    [SerializeField]
    private AudioClip[] infectSfx;

    [Space(10)]
    [SerializeField, Tooltip("Parent object for persistent hosts.")]
    private GameObject persistentHosts;

    [SerializeField]
    private CinemachineFreeLook ParasiteBasicCam;

    private AudioSource AudioSource;

    public bool isLeeching = false;

    #endregion

    #region UNITY METHODS

    private void Update()
    {
        if (Input.GetButtonDown(GameConstants.k_ButtonNameLeech) && !isLeeching && canUse)
        {
            StartCoroutine(Leeching());
        }
    }
    #endregion

    #region ABILITY IMPLEMENTATION
    /// <summary>
    /// Handles the possession ability logic.
    /// </summary>
    public override void Ability()
    {
        Debug.Log("Possession triggered.");

        // Disable the parasite
        this.gameObject.SetActive(false);
    }
    /// <summary>
    /// Coroutine for leeching effect before possession.
    /// </summary>
    private IEnumerator Leeching()
    {
        isLeeching = true;
        Debug.Log("Leeching started.");

        yield return new WaitForSeconds(2f); // Play effects or UI prompts here if needed

        isLeeching = false;
        Debug.Log("Leeching ended.");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isLeeching && hit.gameObject.layer == LayerMask.NameToLayer("AI"))
        {
            // Instantiate blood effect
            ParticleSystem blood = Instantiate(bloodVfx, hit.point, Quaternion.LookRotation(hit.normal));
            blood.transform.SetParent(hit.gameObject.transform);
            Destroy(blood, 1f);

            AudioSource =  hit.gameObject.GetComponentInParent<AudioSource>();
            Game_Manager.PlayRandomSfx(AudioSource, infectSfx);

            Debug.Log($"Attempting to possess: {hit.gameObject.name}");
            Events.ActorPossesedEvent.CurrentActor = hit.gameObject.GetComponent<Actor>().id;

            TriggerAbility();
        }
    }
    #endregion
}