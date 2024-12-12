using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(int levelIndex)
    {
        GameObject player = GameObject.FindWithTag("Player");

        // Load scene
        SceneManager.LoadScene(levelIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.GetComponent<Infected>())
            LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
