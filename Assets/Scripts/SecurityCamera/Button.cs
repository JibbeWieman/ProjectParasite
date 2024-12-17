using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    public UnityEvent ButtonEvent;
    private void OnMouseDown()
    {
        ButtonEvent.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetButtonDown(GameConstants.k_ButtonNameJump))
        {
            ButtonEvent.Invoke();
        }
    }
}
