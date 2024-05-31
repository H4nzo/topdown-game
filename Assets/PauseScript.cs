using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public bool isPaused = false;

    public GameObject pauseCam;
    public GameObject pauseCanvas;
    public GameObject playerCamera;

    private bool wasPaused = false;

    // Update is called once per frame
    void Update()
    {
        if (isPaused && !wasPaused)
        {
            StartCoroutine(startPause(2f));
            wasPaused = true;
        }
        else if (!isPaused && wasPaused)
        {
            StartCoroutine(ResumePause(2f));
            wasPaused = false;
        }
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void Pause()
    {
        isPaused = true;
    }

    IEnumerator startPause(float t)
    {
        pauseCam.SetActive(true);
        playerCamera.SetActive(false);
        pauseCanvas.SetActive(true);
        yield return new WaitForSecondsRealtime(t);
        Time.timeScale = 0;
    }

    IEnumerator ResumePause(float t)
    {
        // Ensure that the time scale is reset immediately to avoid delaying physics updates
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(t);
        pauseCam.SetActive(false);
        playerCamera.SetActive(true);
        pauseCanvas.SetActive(false);
    }
}
