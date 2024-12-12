using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float minDestroyTime = 3f;
    public float maxDestroyTime = 10f;
    private float destroyTime;

    private void Start()
    {
        destroyTime = Random.Range(minDestroyTime, maxDestroyTime); // Get a random value between 3 and 10
        Destroy(gameObject, destroyTime);
    }
}