using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using static HostThirdPersonCam;

public class CameraSwitcher : MonoBehaviour
{
    static List<CinemachineFreeLook> cameras = new List<CinemachineFreeLook>();

    public static CinemachineFreeLook ActiveCamera = null;

    public static bool IsActiveCamera(CinemachineFreeLook camera)
    {
        return camera == ActiveCamera;
    }

    public static void SwitchCamera(CinemachineFreeLook camera)
    {
        ActiveCamera = camera; // Always update the active camera reference
        camera.Priority = 10;

        foreach (CinemachineFreeLook cam in cameras)
        {
            if (cam != camera)
            {
                cam.Priority = 0;
            }
        }
    }


    public static void Register(CinemachineFreeLook camera)
    {
        if (!cameras.Contains(camera))
        {
            cameras.Add(camera);
            Debug.Log("Camera registered: " + camera);
        }
    }

    public static void Unregister(CinemachineFreeLook camera)
    {
        if (cameras.Contains(camera))
        {
            camera.Priority = 0; // Reset priority before removing
            cameras.Remove(camera);
            Debug.Log("Camera unregistered: " + camera);
        }
    }

    public static void RegisterCameras(Actor actor)
    {
        // Reset previously active cameras to avoid conflicts
        foreach (CinemachineFreeLook cam in cameras)
        {
            cam.Priority = 0;
        }

        // Now register the new cameras
        if (actor.BasicCam != null)
        {
            Register(actor.BasicCam);
        }

        if (actor.CombatCam != null)
        {
            Register(actor.CombatCam);
        }
    }


    public static void UnregisterCameras(Actor actor)
    {
        if (actor.BasicCam != null && cameras.Contains(actor.BasicCam))
        {
            Unregister(actor.BasicCam);
        }

        if (actor.CombatCam != null && cameras.Contains(actor.CombatCam))
        {
            Unregister(actor.CombatCam);
        }
    }

}
