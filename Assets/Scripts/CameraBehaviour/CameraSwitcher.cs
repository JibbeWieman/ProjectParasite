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
        camera.Priority = 10;
        ActiveCamera = camera;

        foreach(CinemachineFreeLook cam in cameras)
        {
            if (cam != camera && cam.Priority != 0)
            {
                cam.Priority = 0;
            }
        }
    }

    public static void Register(CinemachineFreeLook camera)
    {
        cameras.Add(camera);
        Debug.Log("Camera registered" + camera);
    }

    public static void Unregister(CinemachineFreeLook camera)
    {
        cameras.Remove(camera);
        Debug.Log("Camera unregistered" + camera);
    }
}
