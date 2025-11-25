using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    private bool isOptionsOpen = false;

    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }


    public void PauseGame()
    {
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);

        Time.timeScale = 0f;

    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        isOptionsOpen = true;
       UnityEngine.Debug.Log("Options Opened");
    }

    public void CloseOptions()
    {
        if (isOptionsOpen)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Screen"); 
    
    }
}
