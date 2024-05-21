using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int missionIndex;
    public GameObject minimapCanvas;
    public GameObject topContainer;


    private void Start()
    {
        Time.timeScale = 1f;
    }
    // Start is called before the first frame update
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(missionIndex);
    }

}
