using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextManager : Manager
{
    [SerializeField] private VoiceLines[] _voiceLines;
    [SerializeField] private VoiceLines[] _condtionComments;

    private VoiceLines _currentVoiceLines;

    IEnumerator Start()
    {
        _promptContinue.enabled = false;

        yield return CheckConditionAmount();

        _currentVoiceLines = _voiceLines[MainManager.Instance.CurrentDay];

        yield return PrintMultipleLines(_currentVoiceLines.Lines);

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
        ListenForSkip();
    }

    private IEnumerator CheckConditionAmount()
    {
        int conditionAmount = ConditionManager.Instance.GetCurrentConditions().Count;

        if (conditionAmount == MainManager.Instance.ConditionAmount || conditionAmount == 0)
        {
            yield break;
        }

        MainManager.Instance.ConditionAmount = conditionAmount;

        VoiceLines voiceLines = _condtionComments[conditionAmount];

        _currentLine = voiceLines.Lines[UnityEngine.Random.Range(0, voiceLines.Lines.Length)];
        yield return HandleTextOutput(_currentLine, false);
    }
}
