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
    private ActorWeaponsManager actorWeaponsManager;

    public Actor currentActor;

    [Tooltip("AKA the parasite")]
    private GameObject player;

    private HostThirdPersonCam hostCam;
    private InfectAbility infectAbility;
    #endregion

    #region UNITY METHODS
    private void Start()
    {
        actorManager = GetComponent<ActorsManager>();
        player = actorManager?.Player;

        EventManager.AddListener<ActorPossesedEvent>(SwitchActor);
        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0

        hostCam = FindObjectOfType<HostThirdPersonCam>();
        DebugUtility.HandleErrorIfNullFindObject<HostThirdPersonCam, ActorFacade>(hostCam, this);

        infectAbility = FindObjectOfType<InfectAbility>();
        DebugUtility.HandleErrorIfNullFindObject<InfectAbility, ActorFacade>(infectAbility, this);
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
    #endregion

    #region ACTOR MANAGEMENT
    public void SwitchActor(ActorPossesedEvent evt)
    {
        Actor targetActor = actorManager.FindActorById(evt.CurrentActor);

        if (targetActor == null)
        {
            Debug.LogError($"Actor with ID {evt.CurrentActor} not found!");
            return;
        }

        if (currentActor != null)
        {
            UnregisterCameras(currentActor);
        }

        EnterActor(targetActor);

        // Is new actor the parasite check
        if (targetActor != actorManager.FindActorById(0))
        {
            player.SetActive(false);
            infectAbility.isLeeching = false;
            Events.ActorPossesedEvent.InHost = true;
        }
        else
        {
            Events.ActorPossesedEvent.InHost = false;
        }

        CameraSwitcher.SwitchCamera(targetActor.BasicCam);
    }

    private void EnterActor(Actor targetActor)
    {
        currentActor = targetActor;

        actorController = currentActor.GetComponent<ActorCharacterController>();
        if (actorController == null)
        {
            Debug.LogError($"ActorCharacterController not found on {currentActor.gameObject.name}!");
            return;
        }

        if (actorController.m_PatrolAgent != null)
        {
            actorController.m_PatrolAgent.enabled = false;
        }

        actorWeaponsManager = currentActor.GetComponent<ActorWeaponsManager>();
        if (actorWeaponsManager)
        {
            actorWeaponsManager.enabled = true;
        }

        hostCam?.SetupHostVariables(currentActor);
        actorController.transform.SetPositionAndRotation(currentActor.transform.position, currentActor.transform.rotation);
        RegisterCameras(currentActor);
    }

    private void LeaveHost()
    {
        player.transform.SetPositionAndRotation(currentActor.transform.position + new Vector3(-1, 0, 0), currentActor.transform.rotation);
        player.SetActive(true);

        if (actorController?.m_PatrolAgent != null)
        {
            actorController.m_PatrolAgent.enabled = true;
        }
        if (actorWeaponsManager != null)
        {
            actorWeaponsManager.enabled = false;
        }

        Transform refugeCamp = GameObject.FindWithTag("HostRefugeCamp")?.transform;
        if (refugeCamp != null)
        {
            transform.SetParent(refugeCamp);
        }

        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0
    }
    #endregion

    #region CAMERA MANAGEMENT
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
    #endregion
}
