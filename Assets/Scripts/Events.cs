using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
    public static ActorPossesedEvent ActorPossesedEvent = new();    
}

// Jibbe's Events
public class ActorPossesedEvent : GameEvent
{
    private int currentActor = 0; // Backing field

    /// <summary>
    /// Gets or sets the ID of the currently possessed actor.
    /// Broadcasts the event whenever the value changes.
    /// </summary>
    public int CurrentActor
    {
        get { return currentActor; }
        set
        {
            currentActor = value;
            EventManager.Broadcast(Events.ActorPossesedEvent);
            Debug.Log($"Setting CurrentActor. Old Value: {currentActor}, New Value: {value}");
        }
    }

    /// <summary>
    /// Indicates if the current actor is the host.
    /// Returns true if CurrentActor is 0, otherwise false.
    /// </summary>
    public bool InHost;
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