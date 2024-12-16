using UnityEngine;
using Cinemachine;
using static HostThirdPersonCam;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ActorFacade : MonoBehaviour
{
    private ActorsManager actorManager; // Reference to the ActorManager
    private ActorCharacterController actorController; // Direct controller reference
    private Actor currentActor;

    private GameObject player; // Player meaning the parasite

    private HostThirdPersonCam hostThirdPersonCam; // Reference to the HostThirdPersonCam

    private Volume globalVolume;

    private void Start()
    {
        actorManager = GetComponent<ActorsManager>();
        player = actorManager.Player;

        EventManager.AddListener<ActorPossesedEvent>(SwitchActor);
        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0

        // Find and store the HostThirdPersonCam reference
        hostThirdPersonCam = FindObjectOfType<HostThirdPersonCam>();
        if (hostThirdPersonCam == null)
        {
            Debug.LogError("HostThirdPersonCam not found in the scene!");
        }

        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogError("HostThirdPersonCam not found in the scene!");
        }
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
                
                if (globalVolume.profile.TryGet<DepthOfField>(out var depthOfField))
                {
                    depthOfField.focusDistance.value = 10f; // Example value for focus distance
                    depthOfField.aperture.value = 5.6f;     // Example value for aperture
                    depthOfField.focalLength.value = 50f;   // Example value for focal length
                }
            }

            currentActor = targetActor;

            actorController = currentActor.GetComponent<ActorCharacterController>();

            if (actorController == null)
            {
                Debug.LogError($"ActorCharacterController not found on {currentActor.gameObject.name}!");
            }

            // Setup host variables in HostThirdPersonCam
            if (hostThirdPersonCam != null)
            {
                hostThirdPersonCam.SetupHostVariables(currentActor);
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

    private void FixedUpdate()
    {
        if (actorController != null)
        {
            MoveActor();
            actorController.AdjustAnimationSpeed();
        }
    }

    public void MoveActor()
    {
        if (currentActor != null)
        {
            actorController.HandleCharacterMovement();
        }
        else
        {
            Debug.LogWarning("No actor is currently active!");
        }
    }
    //private void SwitchCameraStyle(CameraStyle newStyle)
    //{
    //    CombatCam.Priority = 0;
    //    BasicCam.Priority = 0;

    //    if (newStyle == CameraStyle.Basic) BasicCam.Priority = 10;
    //    if (newStyle == CameraStyle.Combat) CombatCam.Priority = 10;

    //    currentStyle = newStyle;
    //}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && InfectAbility.inHost)
            LeaveHost();
    }

    private void LeaveHost()
    {
        // Set parasite to the host's position and rotation
        player.transform.position = currentActor.transform.position;
        player.transform.rotation = currentActor.transform.rotation;

        player.SetActive(true);

        this.transform.SetParent(GameObject.FindWithTag("HostRefugeCamp").transform);

        InfectAbility.inHost = false;
        Events.ActorPossesedEvent.CurrentActor = 0; // Player actor ID is 0
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
