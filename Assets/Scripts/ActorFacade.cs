using UnityEngine;

public class ActorFacade : MonoBehaviour
{
    [SerializeField] private ActorsManager actorManager; // Reference to the ActorManager
    [SerializeField] private PlayerMovement playerController; // Direct controller reference

    private Actor currentActor;

    private void Start()
    {
        if (actorManager == null)
        {
            Debug.LogError("ActorManager is not assigned!");
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerCharacterController is not assigned!");
        }

        EventManager.AddListener<ActorPossesedEvent>(SwitchActor);
    }

    public void SwitchActor(ActorPossesedEvent evt)
    {
        Actor targetActor = actorManager.FindActorById(evt.CurrentActor);

        if (targetActor != null)
        {
            // Unregister cameras from the current actor
            if (currentActor != null)
            {
                UnregisterCameras(currentActor);
            }

            currentActor = targetActor;

            // Sync player position and rotation with the new actor
            playerController.transform.position = currentActor.transform.position;
            playerController.transform.rotation = currentActor.transform.rotation;

            // Register cameras of the new actor
            RegisterCameras(currentActor);

            CameraSwitcher.SwitchCamera(targetActor.BasicCam);
        }
        else
        {
            Debug.LogError($"Actor with ID {evt.CurrentActor} not found!");
        }
    }

    public void MoveActor(Vector3 direction)
    {
        if (currentActor != null)
        {
            //playerController.Move(direction);
        }
        else
        {
            Debug.LogWarning("No actor is currently active!");
        }
    }

    public void JumpActor(float jumpForce)
    {
        if (currentActor != null)
        {
            //playerController.Jump(jumpForce);
        }
        else
        {
            Debug.LogWarning("No actor is currently active!");
        }
    }

    /// <summary>
    /// Registers the cameras for the specified actor.
    /// </summary>
    /// <param name="actor">The actor whose cameras should be registered.</param>
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

    /// <summary>
    /// Unregisters the cameras for the specified actor.
    /// </summary>
    /// <param name="actor">The actor whose cameras should be unregistered.</param>
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
