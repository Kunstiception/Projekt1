using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : Manager
{
    [SerializeField] private string _question;
    [SerializeField] private TextMeshProUGUI _uiTextElement;
    [SerializeField] private Button _buttonLoad;
    [SerializeField] private Button _buttonNew;

    private IEnumerator Start()
    {
        ToggleCursorState(false);
        _buttonLoad.enabled = false;
        _buttonNew.enabled = false;

        yield return DialogueUtil.DisplayTextOverTime(_question, _uiTextElement, null, null);

        _buttonLoad.enabled = true;
        _buttonNew.enabled = true;
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
}
