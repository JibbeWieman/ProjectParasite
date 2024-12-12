using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    public UnityEvent ButtonEvent;
    private void OnMouseDown()
    {
        ButtonEvent.Invoke();
    }
}
