using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartUI : MonoBehaviour
{
    public bool alwaysDisplayMouse;
    public GameObject pauseCanvas;
    public GameObject optionsCanvas;
    public GameObject controlsCanvas;

    protected bool m_InPause;
    protected PlayableDirector[] m_Directors;

    void Start()
    {
        if (!alwaysDisplayMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        m_Directors = FindObjectsByType<PlayableDirector>(FindObjectsSortMode.None);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void ExitPause()
    {
        m_InPause = true;
        SwitchPauseState();
    }

    public void RestartLevel()
    {
        m_InPause = true;
        SwitchPauseState();
        //SceneController.RestartZone();
    }

    void Update()
    {
        //if (PlayerInput.Instance != null && PlayerInput.Instance.Pause)
        //{
        //    SwitchPauseState();
        //}
    }

    protected void SwitchPauseState()
    {
        //if (m_InPause && Time.timeScale > 0 || !m_InPause && ScreenFader.IsFading)
        //    return;

        if (!alwaysDisplayMouse)
        {
            Cursor.lockState = m_InPause ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_InPause;
        }

        for (int i = 0; i < m_Directors.Length; i++)
        {
            if (m_Directors[i].state == PlayState.Playing && !m_InPause)
            {
                m_Directors[i].Pause();
            }
            else if (m_Directors[i].state == PlayState.Paused && m_InPause)
            {
                m_Directors[i].Resume();
            }
        }

        //if (!m_InPause)
        //    CameraShake.Stop();

        //if (m_InPause)
        //    PlayerInput.Instance.GainControl();
        //else
        //    PlayerInput.Instance.ReleaseControl();

        Time.timeScale = m_InPause ? 1 : 0;

        if (pauseCanvas)
            pauseCanvas.SetActive(!m_InPause);

        if (optionsCanvas)
            optionsCanvas.SetActive(false);

        if (controlsCanvas)
            controlsCanvas.SetActive(false);

        m_InPause = !m_InPause;
    }
}
