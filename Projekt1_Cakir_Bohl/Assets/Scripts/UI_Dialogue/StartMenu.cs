using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private string _question;
    [SerializeField] private TextMeshProUGUI _uiTextElement;
    [SerializeField] private Button _buttonLoad;
    [SerializeField] private Button _buttonNew;

    private IEnumerator Start()
    {
        _buttonLoad.enabled = false;
        _buttonNew.enabled = false;

        yield return DialogueUtil.DisplayTextOverTime(_question, _uiTextElement);

        _buttonLoad.enabled = true;
        _buttonNew.enabled = true;
    }

    public void LoadSave()
    {
        MainManager.Instance.LoadAll();

        SceneManager.LoadScene("MapTest");
    }

    public void LoadNew()
    {
        SceneManager.LoadScene("DialogueTest");
    }
}
