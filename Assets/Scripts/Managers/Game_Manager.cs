using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance { get; private set; }

    #region VARIABLES

    [Header("Variables")]

    //Player healthsystem
    //static public HealthSystem playerHealth = new HealthSystem(100);
    //[SerializeField] private CharacterWindow characterWindow;
    //readonly Character character = new Character();

    private GameObject player;

    #endregion

    #region Singleton
    /// <summary>
    /// Sets up singleton
    /// </summary>
    private void Awake()
    {
        // Check if the instance already exists and destroy any duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // If no instance exists, set this as the instance
        Instance = this;

        // Optionally, make this instance persistent across scenes
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        player = GetComponent<ActorsManager>().Player;
    }

    private void Update()
    {
        //if (player == null)
        //{
        //    player = GameObject.FindWithTag("Player").transform.root.gameObject;
        //}

        #region Swap to parasite onces host dies
        //if (InfectAbility.inHost && InfectAbility.host == null)
        //{
        //    player.SetActive(true);
        //    player.GetComponent<PlayerMovement>().enabled = true;
        //    InfectAbility.inHost = false;
        //}
        #endregion
    }

    // A method to start the cooldown
    public void StartCooldown(AbilityBase ability, float cooldown)
    {
        // Start the cooldown process but handle it differently for InfectAbility
        //if (ability is InfectAbility infectAbility)
        //{
        //    StartCoroutine(AbilityCooldownWithCondition(infectAbility, cooldown));
        //}
        //else
        //{
            StartCoroutine(AbilityCooldown(ability, cooldown));
        //}
    }
    private IEnumerator AbilityCooldown(AbilityBase ability, float cooldown)
    {
        ability.canUse = false;
        yield return new WaitForSeconds(cooldown);
        ability.canUse = true;
    }

    /*private IEnumerator AbilityCooldownWithCondition(InfectAbility ability, float cooldown)
    {
        ability.canUse = false;

        float elapsedTime = 0f;

        while (elapsedTime < cooldown)
        {
            // Only increase the elapsed time when not in host
            if (!InfectAbility.inHost)
            {
                elapsedTime += Time.deltaTime;
            }

            yield return null;
        }

        ability.canUse = true;
    }*/
}
