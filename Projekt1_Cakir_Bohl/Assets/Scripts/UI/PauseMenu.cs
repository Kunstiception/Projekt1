using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;

    public static event Action<bool> OnPausedToggled;

    void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            _resumeButton.interactable = false;
            _mainMenuButton.interactable = false;    
        }

        _pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseMenu.activeSelf)
            {
                OnPausedToggled?.Invoke(false);

                _pauseMenu.SetActive(false);

                Time.timeScale = 1;

                // if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(2))
                // {
                //     Cursor.visible = false;
                // }
            }
            else
            {
                OnPausedToggled?.Invoke(true);

                _pauseMenu.SetActive(true);

                Time.timeScale = 0;

                // Cursor.visible = true;
            }
        }
    }

    public void Resume()
    {
        _pauseMenu.SetActive(false);

        OnPausedToggled?.Invoke(false);

        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        
        SceneManager.LoadScene(0);
    }
}
