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
    
    IEnumerator Start()
    {
        _textBox.text = "";
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        SetCondionName();

        yield return new WaitForSeconds(2);

        _currentLine = CreateConditionLine();

        if (_currentLine != null)
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));         
        }

        SceneManager.LoadScene(2);
    }

    void Update()
    {
        ListenForSkip();
    }

    private void SetCondionName()
    {
        switch(PlayerManager.Instance.LatestCondition)
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
                conditionNames.Add($" {conditionName}");

                continue;
            }

            conditionNames.Add($" -{conditionName}");
        }

        foreach (string name in conditionNames)
        {
            line += name;
        }

        line += "!";

        return line;
    }
}