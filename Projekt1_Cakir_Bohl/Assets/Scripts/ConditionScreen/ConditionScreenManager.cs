using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConditionScreenManager : Manager
{
    [SerializeField] private GameObject _sleepDeprivedText;
    [SerializeField] private GameObject _zombieText;
    [SerializeField] private GameObject _werewolfText;
    [SerializeField] private GameObject _vampireText;
    private string _conditionName;
    private bool _isTextDisplayed = false;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        _textBox.text = "";
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;
        _autoArrows.enabled = false;

        SetConditionName();

        yield return new WaitForSeconds(GameConfig.ConditionScreenWaitTime);

        _currentLine = CreateConditionLine();

        if (_currentLine != null)
        {
            _isTextDisplayed = true;

            if (PlayerManager.Instance.IsAuto)
            {
                _autoArrows.enabled = true;
            }

            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }
        else
        {
            yield return new WaitForSeconds(GameConfig.ConditionScreenWaitTime / 3);
        }

        if (!PlayerManager.Instance.HasFinishedDay)
        {
            SceneManager.LoadScene(2);

            yield break;
        }

        SceneManager.LoadScene(1);
    }

    void Update()
    {
        if (!_isTextDisplayed)
        {
            return;
        }

        ListenForSkipOrAuto();
    }

    // Sucht anhand des letzten erlangten Zustands den passenden animierten Text
    private void SetConditionName()
    {
        switch (PlayerManager.Instance.LatestCondition)
        {
            case ConditionManager.Conditions.SleepDeprived:
                _conditionName = "sleep deprived";
                Instantiate(_sleepDeprivedText);

                break;

            case ConditionManager.Conditions.Vampire:
                _conditionName = "Vampire";
                Instantiate(_vampireText);

                break;

            case ConditionManager.Conditions.Werewolf:
                _conditionName = "Werewolf";
                Instantiate(_werewolfText);

                break;

            case ConditionManager.Conditions.Zombie:
                _conditionName = "Zombie";
                Instantiate(_zombieText);

                break;
        }
    }

    // Setzt den passenden Text zusammen, der unter dem animierten Text erscheinen soll
    // Dies passiert aber nur, wenn noch weitere Zustände vorliegen
    private string CreateConditionLine()
    {
        string line = "... a ";

        var conditions = ConditionManager.Instance.GetCurrentConditions();

        if (conditions.Count == 1)
        {
            return null;
        }

        if (conditions.Contains(ConditionManager.Conditions.SleepDeprived))
        {
            line += "sleep deprived ";
        }

        List<string> conditionNames = new List<string>();

        foreach (ConditionManager.Conditions condition in conditions)
        {
            string conditionName = null;

            if (condition == ConditionManager.Conditions.Werewolf && !MainManager.Instance.IsDay)
            {
                conditionName = "Werewolf";
            }

            if (condition == ConditionManager.Conditions.Zombie)
            {
                conditionName = "Zombie";
            }

            if (condition == ConditionManager.Conditions.Vampire)
            {
                conditionName = "Vampire";
            }

            if (conditionName == null)
            {
                continue;
            }

            if (conditionNames.Count == 0)
            {
                conditionNames.Add($"{conditionName}");

                continue;
            }

            conditionNames.Add($"-{conditionName}");
        }

        foreach (string name in conditionNames)
        {
            line += name;
        }

        line += "!";

        return line;
    }
}