using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElevatorDoor : Door
{
    public Collider trigger;
    public int keyRequirement;
    public int keyAmount;

    [SerializeField]
    private TextMeshProUGUI doorStatus;

    private MovingPlatform Elevator;

    public override void Start()
    {
        base.Start();

        Elevator = GetComponentInParent<MovingPlatform>();
        trigger = GetComponent<BoxCollider>();
        trigger.enabled = false;
    }

    public void UpdateKeyAmount()
    {
        keyAmount++;
        doorStatus.text = $"NEEDS {keyAmount} TO RE-ACTIVATE.";

        if (keyAmount >= keyRequirement)
        {
            Unlock();
            doorStatus.text = "";
        }
    }

    public void Unlock()
    {
        trigger.enabled = true;
        EventManager.Broadcast(Events.ElevatorUnlockedEvent);
    }

    public override void HandleDoorInteraction()
    {
        if (Elevator.isMoving) return;

        base.HandleDoorInteraction();
    }
}
