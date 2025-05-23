using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public GameObject parasiteReticle;
    public GameObject hostReticle;
    public GameObject ammoCount;

    [Header("Ability References")]
    public AbilityUI abilityIconPrefab;
    public Transform abilityIconSlot;

    private ActorsManager actorsManager;

    private bool m_abilitiesInitialized = false;

    private void Start()
    {
        actorsManager = FindAnyObjectByType<ActorsManager>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Initialize abilities if not done already
        if (!m_abilitiesInitialized)
        {
            InitializeAbilities();
            m_abilitiesInitialized = true;
        }

        #region Reticle swapping
        if (!Events.ActorPossesedEvent.InHost)
        {
            parasiteReticle.SetActive(true);
            hostReticle.SetActive(false);
        }
        else if (Events.ActorPossesedEvent.InHost)
        {
            parasiteReticle.SetActive(false);
            hostReticle.SetActive(true);
        }
        #endregion

        #region Show Ammo Count
        //Make sure ammo count only shows while holding a gun
        if (Events.ActorPossesedEvent.InHost)
        {
            //if (.GetComponentInChildren<ProjectileGun>())
                //ammoCount.SetActive(true);
        }
        else
        {
            ammoCount.SetActive(false);
        }
        #endregion
    }

    private void InitializeAbilities()
    {
        ActorCharacterController player = actorsManager.Player.GetComponent<ActorCharacterController>();
        player = ActorCharacterController.Instance;

        // Check if player or its m_Abilities array is null
        if (player == null || player.m_Abilities == null)
        {
            Debug.LogError("PlayerMovement instance or m_Abilities is null.");
            return;
        }

        for (int i = 0; i < player.m_Abilities.Length; i++)
        {
            AbilityUI abilityUi = Instantiate(abilityIconPrefab, abilityIconSlot);
            player.m_Abilities[i].OnAbilityUse.AddListener((cooldown) => abilityUi.ShowCoolDown(cooldown));
            abilityUi.SetIcon(player.m_Abilities[i].icon);
        }
    }
}
