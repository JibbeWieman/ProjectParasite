using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    public Collider trigger;
    public int keyRequirement;
    public int keyAmount;

    [SerializeField]
    private TextMeshProUGUI doorStatus;

    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
        trigger.enabled = false;
    }

    public void UpdateKeyAmount()
    {
        keyAmount++;
        doorStatus.text = $"NEEDS {keyAmount} TO RE-ACTIVATE.";

        if (keyAmount >= keyRequirement)
        {
            trigger.enabled = true;
            doorStatus.text = "";
        }
    }
}
