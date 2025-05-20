using TMPro;
using UnityEngine;

public class Keyhole : MonoBehaviour
{
    #region VARIABLES
    [SerializeField]
    private AudioSource AudioSource;

    [SerializeField] 
    private AudioClip CorrectKey, WrongKey;

    [SerializeField]
    private SceneTypeObject ST_ElevatorDoor;

    private ElevatorDoor doorToUnlock;

    private TextMeshProUGUI insertText;

    private bool isKeyInserted = false;

    #endregion

    #region UNITY METHODS
    private void Start()
    {
        insertText = transform.parent.GetComponentInChildren<TextMeshProUGUI>();
        insertText.enabled = false;

        doorToUnlock = ST_ElevatorDoor.Objects[0].GetComponent<ElevatorDoor>();
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
    #endregion

    #region KEY METHODS
    public void InsertKey()
    {
        if (isKeyInserted)
        {
            AudioSource.PlayOneShot(WrongKey);
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

        AudioSource.PlayOneShot(CorrectKey);
        Debug.Log(doorToUnlock);
        doorToUnlock.UpdateKeyAmount();
        Debug.Log("Key inserted into keyhole.");
    }

    #endregion
}
