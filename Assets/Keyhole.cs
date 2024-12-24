using UnityEngine;

public class Keyhole : MonoBehaviour
{
    private bool isKeyInserted = false;

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
}
