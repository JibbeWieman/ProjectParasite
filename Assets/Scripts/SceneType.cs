using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneType : MonoBehaviour
{
    [SerializeField] private SceneTypeObject type;

    public SceneTypeObject GetSceneTypeObject()
    {
        return type;
    }

    private void Start()
    {
        if (type != null)
        {
            type.Add(gameObject);
        }
        else
        {
            Debug.LogWarning("SceneTypeObject is not assigned on " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        type.Remove(gameObject);
    }
}