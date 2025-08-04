using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeforeBossFightManager : Manager
{
    [SerializeField] private GameObject[] _animations;
    [SerializeField] private VoiceLines _lines;

    void Start()
    {
        SetPrompts();

        foreach (GameObject animation in _animations)
        {
            animation.SetActive(false);
        }

        StartCoroutine(PlayEnding());

        // Heilt den Player einmal vollkommen vor dem Bosskampf
        ConditionManager.Instance.ResetStats();
        PlayerManager.Instance.HealthPoints = PlayerManager.Instance.GetStartingHealth();
        PlayerManager.Instance.EgoPoints = PlayerManager.Instance.GetStartingEgo();
    }

    void Update()
    {
        ListenForSkipOrAuto();
    }

    // Speilt nacheinander verknüpft mit dem Text die Baum-Animationen ab
    private IEnumerator PlayEnding()
    {
        for (int i = 0; i < _animations.Length; i++)
        {
            if (i > 0)
            {
                _animations[i - 1].SetActive(false);
            }

            if (i == 1)
            {
                _mainEffectsAudioSource.PlayOneShot(_onHeal);
            }

            _animations[i].SetActive(true);

            _currentLine = _lines.Lines[i];
            yield return HandleTextOutput(_currentLine, false);
        }

        SceneManager.LoadScene(4);
    }
}
