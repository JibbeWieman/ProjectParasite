using UnityEngine;
using UnityEngine.SceneManagement;

public class Destructible : MonoBehaviour
{
    public GameObject destroyedVersion;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Player"))
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        // Find the scene
        Scene currentScene = gameObject.scene;

        if (currentScene.isLoaded)
        {
            // Instantiate the prefab in the target scene
            GameObject instantiatedObject = Instantiate(destroyedVersion, transform.position, transform.rotation);

            // Set the parent of the instantiated object to ensure it's in the correct scene hierarchy
            SceneManager.MoveGameObjectToScene(instantiatedObject, currentScene);

            // Set the instantiated object's parent to match the current object's parent
            instantiatedObject.transform.SetParent(transform.parent);

            // Optionally destroy the current object
            Destroy(gameObject);
        }
    }

}
