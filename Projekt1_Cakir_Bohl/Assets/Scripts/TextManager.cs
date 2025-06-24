using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextManager : Manager
{
    [SerializeField] private VoiceLines[] _voiceLines;

    private VoiceLines _currentVoiceLines;

    IEnumerator Start()
    {
        _promptContinue.enabled = false;

        _currentVoiceLines = _voiceLines[MainManager.Instance.CurrentDay];

        yield return PrintMultipleLines(_currentVoiceLines.Lines);

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(2);
    }

    void Update()
    {
        ListenForSkip();
    }
}
