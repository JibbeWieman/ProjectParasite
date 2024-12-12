using UnityEngine;

public class CameraDisplay : MonoBehaviour
{
    public Texture[] emissionMaps;          // Array of emission maps (Textures)
    public Light[] cameraLights;            // Array of Lights corresponding to each emission map
    public GameObject[] cameras;            // Array of Cameras corresponding to each emission map
    public GameObject display;              // The display GameObject (e.g., a screen or monitor)
    private int currentTextureIndex = 0;    // Index to track the current emission map
    private Texture activeEmissionMap;

    private void Start()
    {
        // Initialize the display with the first emission map
        if (emissionMaps.Length > 0 && display != null)
        {
            activeEmissionMap = emissionMaps[currentTextureIndex];
            Renderer renderer = display.GetComponent<Renderer>();

            // Enable emission and assign the emission map
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetTexture("_EmissionMap", activeEmissionMap);

            // Update emission color (necessary to see the effect)
            renderer.material.SetColor("_EmissionColor", Color.white);

            // Activate the corresponding light and camera
            UpdateCameraLightsAndCameras();

            Debug.Log("Starting with EmissionMap: " + activeEmissionMap.name);
        }
        else
        {
            Debug.LogError("EmissionMaps array is empty or display object is null.");
        }
    }

    public void NextCamera()
    {
        // Cycle to the next emission map
        currentTextureIndex = (currentTextureIndex + 1) % emissionMaps.Length;

        // Assign the new emission map to the display
        activeEmissionMap = emissionMaps[currentTextureIndex];
        Renderer renderer = display.GetComponent<Renderer>();

        // Enable emission and assign the emission map
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetTexture("_EmissionMap", activeEmissionMap);

        // Update emission color (necessary to see the effect)
        renderer.material.SetColor("_EmissionColor", Color.white);

        // Activate the corresponding light and camera
        UpdateCameraLightsAndCameras();

        Debug.Log("Switched to EmissionMap: " + activeEmissionMap.name);
    }

    public void PreviousCamera()
    {
        // Cycle to the previous emission map
        currentTextureIndex = (currentTextureIndex - 1 + emissionMaps.Length) % emissionMaps.Length;

        // Assign the new emission map to the display
        activeEmissionMap = emissionMaps[currentTextureIndex];
        Renderer renderer = display.GetComponent<Renderer>();

        // Enable emission and assign the emission map
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetTexture("_EmissionMap", activeEmissionMap);

        // Update emission color (necessary to see the effect)
        renderer.material.SetColor("_EmissionColor", Color.white);

        // Activate the corresponding light and camera
        UpdateCameraLightsAndCameras();

        Debug.Log("Switched to EmissionMap: " + activeEmissionMap.name);
    }

    private void UpdateCameraLightsAndCameras()
    {
        // Disable all lights and cameras first
        foreach (Light light in cameraLights)
        {
            light.enabled = false;
        }
        foreach (GameObject camera in cameras)
        {
            camera.SetActive(false);
        }

        // Enable the light and camera corresponding to the current emission map
        if (cameraLights.Length > currentTextureIndex)
        {
            cameraLights[currentTextureIndex].enabled = true;
        }
        if (cameras.Length > currentTextureIndex)
        {
            cameras[currentTextureIndex].SetActive(true);
        }
    }
}
