using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneField[] _scenesToUnload;
    [SerializeField] private SceneField[] _scenesToLoad;

    private GameObject _player;
    private GameObject _infectedHost;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _infectedHost = GameObject.FindGameObjectWithTag("Host");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == _player || collision.gameObject == _infectedHost)
        {
            //Load and unload the scenes we want
            LoadScene();
            UnloadScene();
        }
    }

    private void LoadScene()
    {
        for (int i = 0; i < _scenesToLoad.Length; i++)
        {
            bool isSceneLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _scenesToLoad[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }

            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(_scenesToLoad[i], LoadSceneMode.Additive);
            }
        }
    }

    private void UnloadScene()
    {
        for (int i = 0; i < _scenesToUnload.Length; i++)
        {
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _scenesToLoad[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(_scenesToUnload[i]);
                }
            }
        }
    }
}
