using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : Manager
{
    [SerializeField] private Button _loadButton;
    [SerializeField] private GameObject _infoScreen;

    private void Start()
    {
        if (!MainManager.Instance.CheckForSaveFile())
        {
            if (MainManager.Instance.IsDevelopment)
            {
                return;
            }

            _loadButton.interactable = false;
        }

        _infoScreen.SetActive(false);

        ToggleCursorState(false);
    }

    public void LoadSave()
    {
        MainManager.Instance.LoadAll();

        if (MainManager.Instance.IsDevelopment)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            PlayerManager.Instance.HasLoadedGame = true;

            SceneManager.LoadScene(7);
        }

    }

    public void LoadNew()
    {
        MainManager.Instance.RevertAll();

        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToggleInfoScreen()
    {
        _infoScreen.SetActive(_infoScreen.activeSelf ? false : true);
    }

    public override void ListenForSkipOrAuto()
    {
        return;
    }
}
