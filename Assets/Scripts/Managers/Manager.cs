using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    public virtual void Start()
    {
        Debug.Log($"Starting process: <color=#FF0000>{this.GetType()}</color>");
    }

    public virtual void Update()
    {
        
    }

    public abstract void Pause();
    // .cpp - Definitions (functions, variables)
    // .h - Declarations (Declare variable / function)
}