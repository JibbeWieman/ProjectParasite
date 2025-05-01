using System.Collections;
using UnityEngine;

public class AddBean : MonoBehaviour
{
    public GameObject targetObject; // Assign in the Inspector

    void Start()
    {
        StartCoroutine(EnableObjectAfterDelay());
    }

    IEnumerator EnableObjectAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }
}