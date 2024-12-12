using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float DestroyTime = 1f;
    public Vector3 Offset = new Vector3(0, 2, 0);
    public Vector3 RandomizeIntensity = new Vector3(0.5f, 0, 0);

    public GameObject player;
    public Camera cam;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        cam = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        Destroy(gameObject, DestroyTime);

        transform.localPosition += Offset;
        transform.localPosition += new Vector3(Random.Range(-RandomizeIntensity.x, RandomizeIntensity.x),
        Random.Range(-RandomizeIntensity.y, RandomizeIntensity.y),
        Random.Range(-RandomizeIntensity.z, RandomizeIntensity.z));
    }

    void LateUpdate()
    {
        // Make the floating text look at the camera's direction
        Vector3 lookDirection = cam.transform.position + cam.transform.forward * 10f; // Arbitrary distance
        transform.LookAt(lookDirection);
    }
}