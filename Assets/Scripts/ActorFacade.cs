using UnityEngine;
using Cinemachine;
using static HostThirdPersonCam;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        if (Input.GetKeyDown(KeyCode.F) && Events.ActorPossesedEvent.InHost)
            LeaveHost();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<ActorPossesedEvent>(SwitchActor);
    }

    public void SwitchActor(ActorPossesedEvent evt)
    {
        Actor targetActor = actorManager.FindActorById(evt.CurrentActor);

        if (targetActor != null)
        {
            if (currentActor != null)
            {
                UnregisterCameras(currentActor);
            }

            if (targetActor != actorManager.FindActorById(0))
            {
                player.SetActive(false); // May need further checks here
                Events.ActorPossesedEvent.InHost = true;
            }

            currentActor = targetActor;

            actorController = currentActor.GetComponent<ActorCharacterController>();

            if (actorController == null)
            {
                Debug.LogError($"ActorCharacterController not found on {currentActor.gameObject.name}!");
            }

            // Setup host variables in HostThirdPersonCam
            if (hostCam != null)
            {
                hostCam.SetupHostVariables(currentActor);
            }

            // Sync player position and rotation with the new actor
            actorController.transform.SetPositionAndRotation(currentActor.transform.position, currentActor.transform.rotation);

            // Register cameras of the new actor
            RegisterCameras(currentActor);

            CameraSwitcher.SwitchCamera(targetActor.BasicCam);
        }
        else
        {
            Debug.LogError($"Actor with ID {evt.CurrentActor} not found!");
        }
    }

    private void LeaveHost()
    {
        player.transform.SetPositionAndRotation(currentActor.transform.position, currentActor.transform.rotation);
        player.SetActive(true);

        Transform refugeCamp = GameObject.FindWithTag("HostRefugeCamp")?.transform;
        if (refugeCamp != null)
            transform.SetParent(refugeCamp);

        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0
        Events.ActorPossesedEvent.InHost = false;
    }

    private void RegisterCameras(Actor actor)
    {
        if (actor.BasicCam != null)
        {
            CameraSwitcher.Register(actor.BasicCam);
        }

        if (actor.CombatCam != null)
        {
            CameraSwitcher.Register(actor.CombatCam);
        }
    }

    private void UnregisterCameras(Actor actor)
    {
        if (actor.BasicCam != null)
        {
            CameraSwitcher.Unregister(actor.BasicCam);
        }

        if (actor.CombatCam != null)
        {
            CameraSwitcher.Unregister(actor.CombatCam);
        }
    }
}
