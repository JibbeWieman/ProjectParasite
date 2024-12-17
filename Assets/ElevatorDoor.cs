using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    public Collider trigger;
    public int keyRequirement; 
    public int keyAmount;

    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
        trigger.enabled = false;
    }

    public void UpdateKeyAmount(int key)
    {
        keyAmount += key;

        if (keyAmount >= keyRequirement)
        {
            trigger.enabled = true;
        }
    }
}
