using Cinemachine;
using UnityEngine;

// This class contains general information describing an actor (player or enemies).
// It is mostly used for AI detection logic and determining if an actor is friend or foe
public class Actor : MonoBehaviour
{
    public int id;

    [Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to each other")]
    public int Affiliation;

    [Tooltip("Represents point where other actors will aim when they attack this actor")]
    public Transform AimPoint;

    ActorsManager m_ActorsManager;

    [Tooltip("The basic camera for this actor"), HideInInspector]
    public CinemachineFreeLook BasicCam;

    [Tooltip("The combat camera for this actor (if applicable)"), HideInInspector]
    public CinemachineFreeLook CombatCam;

    void Awake()
    {
        m_ActorsManager = GameObject.FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, Actor>(m_ActorsManager, this);

        // Automatically assign cameras based on child names
        AssignCameras();

        // Register as an actor
        if (!m_ActorsManager.Actors.Contains(this))
        {
            m_ActorsManager.Actors.Add(this);
        }

        //EventManager.AddListener<AimEvent>(SwitchCamera);
    }

    /// <summary>
    /// Assigns the cameras based on child names or tags to ensure correct assignment.
    /// </summary>
    private void AssignCameras()
    {
        CinemachineFreeLook[] cameras = GetComponentsInChildren<CinemachineFreeLook>(true);

        foreach (CinemachineFreeLook cam in cameras)
        {
            if (cam.gameObject.name.Contains("Basic"))
            {
                BasicCam = cam;
            }
            else if (cam.gameObject.name.Contains("Combat"))
            {
                CombatCam = cam;
            }
        }

        // Optional: Log a warning if the expected cameras are missing
        if (!BasicCam)
        {
            Debug.LogWarning($"Basic Camera not found for {gameObject.name}");
        }

        if (!CombatCam && Affiliation != 0) // Assume the player only has a Basic Cam (Affiliation 0)
        {
            Debug.LogWarning($"Combat Camera not found for {gameObject.name}");
        }
    }

    public void SetID(int idNumber)
    {
        id = idNumber;
    }

    public bool IsActive()
    {
        if (id == Events.ActorPossesedEvent.CurrentActor)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsPlayer()
    {
        if (id == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnDestroy()
    {
        // Unregister as an actor
        if (m_ActorsManager)
        {
            m_ActorsManager.Actors.Remove(this);
        }

        //EventManager.RemoveListener<AimEvent>(SwitchCamera);
    }
}