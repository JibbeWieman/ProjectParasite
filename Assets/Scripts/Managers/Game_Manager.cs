using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour
{
    #region VARIABLES
    public static Game_Manager Instance { get; private set; }

    [Header("Lighting & Environment")]
    [SerializeField] private Color _environmentColor = Color.black; // Default ambient light color
    [SerializeField] private Material _skyboxMaterial; // Assign in Inspector
    [SerializeField] private LightingSettings _lightingSettingsAsset; // Optional: Assign the scene's main directional light


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
        //player = GetComponent<ActorsManager>().Player;
        EventManager.Broadcast(Events.GameStartEvent);

        ApplyLightingSettingsToAllScenes();
    }

    private void ApplyLightingSettingsToAllScenes()
    {
        // Loop through all loaded scenes and apply lighting settings
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                ApplyLightingToScene(scene);
            }
        }
    }

    /// <summary>
    /// Applies lighting settings to a specific scene.
    /// </summary>
    private void ApplyLightingToScene(Scene scene)
    {
        // Ensure the correct scene is active before applying settings
        SceneManager.SetActiveScene(scene);

        // Apply Skybox
        if (_skyboxMaterial != null)
        {
            RenderSettings.skybox = _skyboxMaterial;
        }

        // Apply Ambient Lighting
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = _environmentColor;

        // Apply Light Settings
        //if (_lightingSettingsAsset != null)
        //{
        //    Lightmapping.lightingSettings = _lightingSettingsAsset;
        //}

        // Update global illumination
        DynamicGI.UpdateEnvironment();
    }

    //private void Update()
    //{
    //if (player == null)
    //{
    //    player = GameObject.FindWithTag("Player").transform.root.gameObject;
    //}

    //#region Swap to parasite onces host dies
    //if (InfectAbility.inHost && InfectAbility.host == null)
    //{
    //    player.SetActive(true);
    //    player.GetComponent<PlayerMovement>().enabled = true;
    //    InfectAbility.inHost = false;
    //}
    //#endregion
    //}

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
