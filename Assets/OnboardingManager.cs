using UnityEngine;
using TMPro;

public class OnboardingManager : MonoBehaviour
{
    [SerializeField] private Collider ventTriggerCollider; // Assign the trigger collider in the inspector
    [SerializeField] private TextMeshProUGUI ventOnboardingText; // Assign the UI text

    private void Start()
    {
        if (ventOnboardingText != null)
        {
            ventOnboardingText.gameObject.SetActive(false); // Ensure text is off by default
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other == ventTriggerCollider) // Check if the entering collider is the assigned trigger
        //{
            if (ventOnboardingText != null)
            {
                ventOnboardingText.gameObject.SetActive(true); // Enable the text
            }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other == ventTriggerCollider) // Check if the exiting collider is the assigned trigger
        //{
            if (ventOnboardingText != null)
            {
                ventOnboardingText.gameObject.SetActive(false); // Disable the text
            }
        //}
    }
}
