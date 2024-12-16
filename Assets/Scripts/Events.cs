using UnityEngine;

// The Game Events used across the Game.
// Anytime there is a need for a new event, it should be added here.

public static class Events
{
    public static GameOverEvent GameOverEvent = new();
    public static PlayerDeathEvent PlayerDeathEvent = new();
    public static EnemyKillEvent EnemyKillEvent = new();
    public static PickupEvent PickupEvent = new();
    public static AmmoPickupEvent AmmoPickupEvent = new();
    public static DamageEvent DamageEvent = new();
    public static DisplayMessageEvent DisplayMessageEvent = new();
        
    // Jibbe's Events
    public static ActorAssignedEvent ActorAssignedEvent = new();    
    public static ActorPossesedEvent ActorPossesedEvent = new();    
}

// Jibbe's Events
public class ActorAssignedEvent : GameEvent
{
    private int actorAmount; // Backing field

    public int ActorAmount
    {
        get { return actorAmount; }
        set
        {
            actorAmount = value;
            EventManager.Broadcast(Events.ActorAssignedEvent);
        }
    }
}

public class ActorPossesedEvent : GameEvent
{
    private int currentActor = 0; // Backing field

    public int CurrentActor
    {
        get { return currentActor; }
        set
        {
            currentActor = value;
            EventManager.Broadcast(Events.ActorPossesedEvent);
        }
    }
}

// FPS Micro

public class GameOverEvent : GameEvent
{
    public bool Win;
}

public class PlayerDeathEvent : GameEvent { }

public class EnemyKillEvent : GameEvent
{
    public GameObject Enemy;
    public int RemainingEnemyCount;
}

public class PickupEvent : GameEvent
{
    public GameObject Pickup;
}

public class AmmoPickupEvent : GameEvent
{
    public WeaponController Weapon;
}

public class DamageEvent : GameEvent
{
    public GameObject Sender;
    public float DamageValue;
}

public class DisplayMessageEvent : GameEvent
{
    public string Message;
    public float DelayBeforeDisplay;
}