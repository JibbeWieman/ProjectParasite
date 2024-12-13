//using System.Collections;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

//public class VignetteOnHit : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private VolumeProfile volumeProfile;
//    //[SerializeField] private ParasiteHP parasiteHP;  // Reference to ParasiteHP script

//    [Header("Vignette Settings")]
//    [SerializeField] private float vignetteBlackIntensity = 0.2f;
//    [SerializeField] private float vignetteMediumRedIntensity = 0.3f;
//    [SerializeField] private float vignetteHighRedIntensity = 0.5f;
//    [SerializeField] private float vignetteTransitionDuration = 0.3f;  // Duration of the intensity transition

//    private Vignette vignette;
//    private Color myRed;
//    private readonly Color transparentRed = new Color(229f / 255f, 30f / 255f, 37f / 255f, 0f); // Red with 0 alpha

//    private float currentIntensity;
//    private Color currentColor;

//    // Singleton Instance
//    public static VignetteOnHit Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        // Set the hexadecimal color to the desired color
//        ColorUtility.TryParseHtmlString("#E51E25", out myRed);
//    }

//    private void Start()
//    {
//        // Find and set up volume and vignette
//        var volume = FindObjectOfType<Volume>();
//        volume.profile = volumeProfile;
//        volumeProfile.TryGet(out vignette);

//        // Initialize with default black vignette
//        currentColor = transparentRed;
//        currentIntensity = vignetteBlackIntensity;
//        vignette.color.value = currentColor;
//        vignette.intensity.value = currentIntensity;
//    }

//    private void Update()
//    {
//        UpdateVignetteBasedOnHealth();
//    }

//    private void UpdateVignetteBasedOnHealth()
//    {
//        //float healthPercentage = (float)parasiteHP.m_CurrentHealth / parasiteHP.m_MaxHealth;

//        float targetIntensity;
//        Color targetColor;

//        if (healthPercentage > 0.66f)
//        {
//            // High health, black vignette
//            targetIntensity = vignetteBlackIntensity;
//            targetColor = transparentRed;
//        }
//        else if (healthPercentage > 0.33f)
//        {
//            // Medium health, medium red vignette
//            targetIntensity = vignetteMediumRedIntensity;
//            targetColor = myRed;
//        }
//        else
//        {
//            // Low health, intense red vignette
//            targetIntensity = vignetteHighRedIntensity;
//            targetColor = myRed;
//        }

//        // Start lerping if the target values are different from the current values
//        if (targetIntensity != currentIntensity || targetColor != currentColor)
//        {
//            StartCoroutine(LerpVignette(targetColor, targetIntensity));
//            currentIntensity = targetIntensity;
//            currentColor = targetColor;
//        }
//    }

//    private IEnumerator LerpVignette(Color targetColor, float targetIntensity)
//    {
//        float elapsedTime = 0f;
//        Color startColor = vignette.color.value;
//        float startIntensity = vignette.intensity.value;

//        while (elapsedTime < vignetteTransitionDuration)
//        {
//            vignette.color.value = Color.Lerp(startColor, targetColor, elapsedTime / vignetteTransitionDuration);
//            vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / vignetteTransitionDuration);

//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }

//        // Ensure final values are set
//        vignette.color.value = targetColor;
//        vignette.intensity.value = targetIntensity;
//    }
//}
