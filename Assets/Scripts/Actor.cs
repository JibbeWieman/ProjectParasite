using Cinemachine;
using UnityEngine;

// This class contains general information describing an actor (player or enemies).
// It is mostly used for AI detection logic and determining if an actor is friend or foe
public class Actor : MonoBehaviour
{
    #region VARIABLES
    public int id;

    [Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to each other")]
    public int Affiliation;

    [Tooltip("Point where other actors will aim when they attack this actor")]
    public Transform AimPoint;

    [Tooltip("The basic camera for this actor"), HideInInspector]
    public CinemachineFreeLook BasicCam { get; private set; }

    [Tooltip("The combat camera for this actor (if applicable)"), HideInInspector]
    public CinemachineFreeLook CombatCam { get; private set; }

    private ActorsManager m_ActorsManager;
    #endregion

    #region UNITY METHODS
    private void Awake()
    {
        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, Actor>(m_ActorsManager, this);

        AssignCameras();

        if (!m_ActorsManager.Actors.Contains(this))
            m_ActorsManager.Actors.Add(this);
    }

    private void OnDestroy() => m_ActorsManager?.Actors.Remove(this);
    #endregion

    #region CAMERA ASSIGNMENT
    /// <summary>
    /// Assigns cameras automatically based on child names or tags.
    /// </summary>
    private void AssignCameras()
    {
        CinemachineFreeLook[] cameras = GetComponentsInChildren<CinemachineFreeLook>(true);

        foreach (CinemachineFreeLook cam in cameras)
        {
            if (cam.name.Contains("Basic")) BasicCam = cam;
            else if (cam.name.Contains("Combat")) CombatCam = cam;

            DebugUtility.HandleErrorIfNullGetComponent<CinemachineFreeLook, Actor>(cam, this, gameObject);
        }
    }
    #endregion

    public void SetID(int idNumber)
    {
        id = idNumber;
    }
}