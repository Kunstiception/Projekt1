using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConditionScreenManager : Manager
{
    private string _conditionName;
    
    IEnumerator Start()
    {
        SetCondionName();

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
                
                break;

            case ConditionManager.Conditions.Werewolf:
                _conditionName = "Werewolf";

                break;

            case ConditionManager.Conditions.Zombie:
                _conditionName = "Zombie";

                break;
        }
    }

    private List<string> CreateLines()
    {
        List<string> lines = new List<string>
        {
            $"You are now a {_conditionName}"
        };

        var conditions = ConditionManager.Instance.EvaluateCurrentConditions();

        if(conditions.Count < 2)
        {
            return lines;
        }

        lines.Add($"... a {_conditionName}");

        foreach(ConditionManager.Conditions condition in conditions)
        {
                if(condition != PlayerManager.Instance.LatestCondition)
                {
                    if(condition == ConditionManager.Conditions.SleepDeprived)
                    {
                        lines[1] = $"... a sleep deprived {_conditionName}";

                        continue;
                    }

                    lines[1] = lines[1] + $"-{condition}";
                }
            }

       lines[1] = lines[1] + "!";

       return lines;
    }
}