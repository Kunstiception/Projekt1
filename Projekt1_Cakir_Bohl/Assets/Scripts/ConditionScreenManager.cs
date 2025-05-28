using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConditionScreenManager : Manager
{
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

        yield return StartCoroutine(PrintMultipleLines(CreateLines().ToArray()));

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

    private List<string> CreateLines()
    {
        List<string> lines = new List<string>();

        var conditions = ConditionManager.Instance.GetCurrentConditions();
        
        if(PlayerManager.Instance.LatestCondition == ConditionManager.Conditions.SleepDeprived)
        {
            lines.Add($"You are now sleep deprived.");
        }

        foreach(ConditionManager.Conditions condition in conditions)
        {
            if(condition != PlayerManager.Instance.LatestCondition)
            {
                if(condition == ConditionManager.Conditions.SleepDeprived)
                {
                    lines.Add($"... a sleep deprived {_conditionName}");

                    continue;
                }

                lines[1] = lines[1] + $"-{condition}";
            }
        }

        if (lines.Count > 1)
        {
            lines[1] = lines[1] + "!";
        }

       return lines;
    }
}