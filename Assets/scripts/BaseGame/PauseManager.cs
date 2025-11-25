using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;

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
       UnityEngine.Debug.Log("Options Opened");
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Screen"); 
    
    }
}
