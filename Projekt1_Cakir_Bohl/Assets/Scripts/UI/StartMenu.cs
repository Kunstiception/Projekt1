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
            _loadButton.interactable = false;
        }

        _infoScreen.SetActive(false);

        ToggleCursorState(false);
    }

    public void LoadSave()
    {
        MainManager.Instance.LoadAll();

        SceneManager.LoadScene(2);
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
