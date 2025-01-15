using TMPro;
using UnityEngine;

public class Keyhole : MonoBehaviour
{
    private bool isKeyInserted = false;

    [SerializeField]
    private TextMeshProUGUI insertText;

    private void Start()
    {
        insertText.enabled = false;
    }

    public void InsertKey()
    {
        if (isKeyInserted)
        {
            Debug.Log("Key already inserted in this keyhole.");
            return;
        }

        isKeyInserted = true;

        // Change the material colour to green
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }

        Debug.Log("Key inserted into keyhole.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter triggered by: {other.gameObject.name}");
        if (other.gameObject.CompareTag("Host"))
        {
            Debug.Log("Host entered trigger. Enabling text.");
            if (insertText != null)
            {
                insertText.enabled = true;
            }
            else
            {
                Debug.LogWarning("insertText is not assigned in the Inspector.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit triggered by: {other.gameObject.name}");
        if (insertText != null && insertText.enabled)
        {
            insertText.enabled = false;
        }
        else
        {
            Debug.LogWarning("insertText is not assigned in the Inspector.");
        }
    }
}
