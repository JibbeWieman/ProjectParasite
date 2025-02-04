using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Objects")]
    [SerializeField] private GameObject _loadingBarObject;
    [SerializeField] private Image _menuBackground;
    [SerializeField] private Image _loadingBar;
    [SerializeField, Tooltip("MainMenu UI elements which need to be hidden upon clicking start")]
    private GameObject[] _objectsToHide;

    [Header("Scenes to Load")]
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _levelScene;

    private List<AsyncOperation> _scenesToLoad = new();

    private void Awake()
    {
        _loadingBarObject.SetActive(false);
    }

    public void StartGame()
    {
        Debug.Log("StartGame() called - Starting game sequence.");

        // Hide the menu UI
        HideMenu();
        _loadingBarObject.SetActive(true);

        // Start loading the main level scene first
        StartCoroutine(LoadScenes());
    }

    private IEnumerator LoadScenes()
    {
        Debug.Log($"Loading level scene: {_levelScene.SceneName}");

        AsyncOperation levelLoad = SceneManager.LoadSceneAsync(_levelScene.SceneName, LoadSceneMode.Additive);
        if (levelLoad == null)
        {
            Debug.LogError($"Failed to load scene: {_levelScene.SceneName}. Check if it's added to Build Settings.");
            yield break;
        }

        _scenesToLoad.Add(levelLoad);

        while (!levelLoad.isDone)
        {
            Debug.Log($"Loading {_levelScene.SceneName}: {levelLoad.progress * 100}%");
            _loadingBar.fillAmount = levelLoad.progress;
            yield return null;
        }

        Debug.Log($"Level scene {_levelScene.SceneName} loaded successfully!");

        // Now load the persistent gameplay scene
        Debug.Log($"Loading persistent gameplay scene: {_persistentGameplay.SceneName}");

        AsyncOperation persistentLoad = SceneManager.LoadSceneAsync(_persistentGameplay.SceneName, LoadSceneMode.Additive);
        if (persistentLoad == null)
        {
            Debug.LogError($"Failed to load scene: {_persistentGameplay.SceneName}. Check if it's added to Build Settings.");
            yield break;
        }

        _scenesToLoad.Add(persistentLoad);

        while (!persistentLoad.isDone)
        {
            Debug.Log($"Loading {_persistentGameplay.SceneName}: {persistentLoad.progress * 100}%");
            _loadingBar.fillAmount = 0.5f + (persistentLoad.progress * 0.5f); // Adjust for second scene
            yield return null;
        }

        Debug.Log($"Persistent gameplay scene {_persistentGameplay.SceneName} loaded successfully!");

        // Finalize loading
        _loadingBar.fillAmount = 1f;
        _menuBackground.color = Color.black;
        Debug.Log("All scenes loaded successfully.");

        SceneManager.UnloadSceneAsync("MainMenu");
    }

    private void HideMenu()
    {
        Debug.Log("Hiding menu UI elements.");
        foreach (var obj in _objectsToHide)
        {
            obj.SetActive(false);
        }
    }
}
