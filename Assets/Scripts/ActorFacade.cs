using UnityEngine;

public class ActorFacade : MonoBehaviour
{
    #region VARIABLES
    private ActorsManager actorManager;
    private ActorCharacterController actorController;
    
    public Actor currentActor;

    [Tooltip("AKA the parasite")]
    private GameObject player;

    private HostThirdPersonCam hostCam;
    #endregion

    private void Start()
    {
        actorManager = GetComponent<ActorsManager>();
        player = actorManager?.Player;

        EventManager.AddListener<ActorPossesedEvent>(SwitchActor);
        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0

        hostCam = FindObjectOfType<HostThirdPersonCam>();
        DebugUtility.HandleErrorIfNullFindObject<HostThirdPersonCam, ActorFacade>(hostCam, this);
    }
    private void FixedUpdate()
    {
        actorController?.HandleCharacterMovement();
        actorController?.AdjustAnimationSpeed();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentActor != actorManager.Player)
            LeaveHost();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<ActorPossesedEvent>(SwitchActor);
    }

    #region ACTOR SWITCHING
    /// <summary>
    /// Handles the switching of control between actors.
    /// </summary>
    /// <param name="evt">Event containing the target actor ID.</param>
    public void SwitchActor(ActorPossesedEvent evt)
    {
        Actor targetActor = actorManager.FindActorById(evt.CurrentActor);

        if (targetActor == null)
        {
            Debug.LogError($"Actor with ID {evt.CurrentActor} not found!");
            return;
        }

        if (targetActor != currentActor)
        {
            if (currentActor != null)
            {
                UnregisterCameras(currentActor);
            }
        }

        currentActor = targetActor;
        actorController = currentActor.GetComponent<ActorCharacterController>();

        if (actorController == null)
        {
            Debug.LogError($"ActorCharacterController missing on {currentActor.name}!");
            return;
        }

        if (Events.ActorPossesedEvent.InHost)
        {
            hostCam?.SetupHostVariables(currentActor);
            actorController.transform.SetPositionAndRotation(currentActor.transform.position, currentActor.transform.rotation);
        }

        RegisterCameras(currentActor);
        CameraSwitcher.SwitchCamera(targetActor.BasicCam);
    }

    /// <summary>
    /// Detaches the parasite from the host, returning control to the player.
    /// </summary>
    private void LeaveHost()
    {
        player.transform.SetPositionAndRotation(currentActor.transform.position, currentActor.transform.rotation);
        player.SetActive(true);

        Transform refugeCamp = GameObject.FindWithTag("HostRefugeCamp")?.transform;
        if (refugeCamp != null)
            transform.SetParent(refugeCamp);

        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0
    }
    #endregion

    #region CAMERA HANDLING
    private void RegisterCameras(Actor actor)
    {
        if (actor == null) return;

        CameraSwitcher.Register(actor.BasicCam);
        CameraSwitcher.Register(actor.CombatCam);
    }

    private void UnregisterCameras(Actor actor)
    {
        if (actor == null) return;

        CameraSwitcher.Unregister(actor.BasicCam);
        CameraSwitcher.Unregister(actor.CombatCam);
    }
    #endregion
}