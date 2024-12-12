using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    private Image fadeImage;

    // Start is called before the first frame update
    void Start()
    {
        fadeImage = GetComponent<Image>();
    }

    /// <summary>
    /// Starts the fading of the image
    /// </summary>
    public void StartFading()
    {
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        // define the sart alpha value
        float alpha = 0; 
        // loop, fading out every step of the loop
        // and pause the loop until the next frame update
        // loop continues while alpha <= 1 

        while (alpha <= 1)
        {
            alpha += Time.deltaTime;
            Color newColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            fadeImage.color = newColor;
            yield return null;
        }

    }
}
