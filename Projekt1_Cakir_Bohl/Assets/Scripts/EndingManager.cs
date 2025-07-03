using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : Manager
{
    [SerializeField] private GameObject[] _animations;
    [SerializeField] private VoiceLines _lines;

    void Start()
    {
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        foreach (GameObject animation in _animations)
        {
            animation.SetActive(false);
        }

        StartCoroutine(PlayEnding());

        ConditionManager.Instance.ResetStats();

        PlayerManager.Instance.HealthPoints = PlayerManager.Instance.GetStartingHealth();

        PlayerManager.Instance.EgoPoints = PlayerManager.Instance.GetStartingEgo();
    }

    void Update()
    {
        ListenForSkip();
    }

    private IEnumerator PlayEnding()
    {
        for (int i = 0; i < _animations.Length; i++)
        {
            if (i > 0)
            {
                _animations[i - 1].SetActive(false);
            }

            _animations[i].SetActive(true);

            _currentLine = _lines.Lines[i];
            yield return HandleTextOutput(_currentLine, false);
        }

        SceneManager.LoadScene(4);
    }
}
