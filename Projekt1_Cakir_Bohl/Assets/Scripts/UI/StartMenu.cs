using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : Manager
{
    [SerializeField] private Button _loadButton;
    [SerializeField] private GameObject _infoScreen;

    private void Start()
    {
        // Falls kein korrektes Savefile vorhanden, wird der Continue-Button ausgegraut
        // Im Development-Modus ist er aber akitv
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

    // Lädt je nach aktivem Speichersystem den letzten Spielstand
    public void LoadSave()
    {
        MainManager.Instance.LoadAll();

        PlayerManager.Instance.ResetPlayer();

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

    // Lädt einen neuen Spielstand und setzt alle Paramter im MainManager zurück
    public void LoadNew()
    {
        MainManager.Instance.RevertAll();

        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Schaltet den Infoscreen ein und aus
    public void ToggleInfoScreen()
    {
        _infoScreen.SetActive(_infoScreen.activeSelf ? false : true);
    }

    public override void ListenForSkipOrAuto()
    {
        return;
    }
}
