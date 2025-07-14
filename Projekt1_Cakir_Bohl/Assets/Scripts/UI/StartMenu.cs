using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : Manager
{
    [SerializeField] private string _question;
    [SerializeField] private TextMeshProUGUI _uiTextElement;
    [SerializeField] private Button _buttonLoad;
    [SerializeField] private GameObject _infoScreen;

    private void Start()
    {
        if (!MainManager.Instance.CheckForSaveFile())
        {
            _buttonLoad.interactable = false;
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
