using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VoiceManager : Manager
{
    [SerializeField] private VoiceLines[] _voiceLines;
    [SerializeField] private VoiceLines[] _condtionComments;

    private VoiceLines _currentVoiceLines;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        SetPrompts();

        _currentVoiceLines = _voiceLines[MainManager.Instance.CurrentDay];

        _currentLine = _currentVoiceLines.Lines[0];
        yield return HandleTextOutput(_currentLine, false);

        yield return CheckConditionAmount();

        for (int i = 1; i < _currentVoiceLines.Lines.Length; i++)
        {
            _currentLine = _currentVoiceLines.Lines[i];
            yield return HandleTextOutput(_currentLine, false);
        }

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        // Bosskampf nach dem letzten Tag
        if (MainManager.Instance.CurrentDay == GameConfig.TotalNumberOfDays)
        {
            SceneManager.LoadScene(11);

            yield break;
        }

        SceneManager.LoadScene(2);
    }

    void Update()
    {
        ListenForSkipOrAuto();
    }

    // Überprüft, wie viele Zustände der Player gerade hat und passt den Kommentar daran an
    private IEnumerator CheckConditionAmount()
    {
        int conditionAmount = ConditionManager.Instance.GetCurrentConditions().Count;

        if (conditionAmount == MainManager.Instance.ConditionAmount || conditionAmount == 0)
        {
            yield break;
        }

        MainManager.Instance.ConditionAmount = conditionAmount;

        VoiceLines voiceLines = _condtionComments[conditionAmount - 1];

        _currentLine = voiceLines.Lines[UnityEngine.Random.Range(0, voiceLines.Lines.Length)];
        yield return HandleTextOutput(_currentLine, false);
    }
}
