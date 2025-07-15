using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : Manager
{
    IEnumerator Start()
    {
        ToggleCursorState(true);

        SetPrompts();

        _textBox.text = "";

        yield return PrintMultipleLines(CreateLines());

        _textBox.text = "";

        yield return new WaitForSeconds(1);

        yield return PrintMultipleLines(UIDialogueStorage.EndLines);

        SceneManager.LoadScene(0);
    }

    private string[] CreateLines()
    {
        List<string> lines = new List<string>();

        lines.Add("On your journey you have...");

        // Enemies
        if (MainManager.Instance.NumberOfDefeatedEnemies != 1)
        {
            lines.Add($"... defeated {MainManager.Instance.NumberOfDefeatedEnemies} enemies.");
        }
        else
        {
            lines.Add("... defeated one enemy.");
        }

        // Villagers
        if (MainManager.Instance.NumberOfVillagersMet != 1)
        {
            lines.Add($"... met {MainManager.Instance.NumberOfVillagersMet} villagers.");
        }
        else
        {
            lines.Add("... met one villager.");
        }

        // Nights
        if (MainManager.Instance.NumberOfNightsSlept != 1)
        {
            lines.Add($"... slept through {MainManager.Instance.NumberOfNightsSlept} nights.");
        }
        else
        {
            lines.Add("... slept through one night.");
        }

        // Conditions
        if (MainManager.Instance.ObtainedConditions.Count > 0)
        {
            lines.Add("All while being...");

            foreach (string condition in MainManager.Instance.ObtainedConditions)
            {
                if (condition == ConditionManager.Instance.SleepDeprivedTerm)
                {
                    lines.Add($"... sleep deprived.");

                    continue;
                }

                if (condition == ConditionManager.Instance.VampireTerm)
                {
                    if (MainManager.Instance.NumberOfPeopleBitten != 1)
                    {
                        lines.Add($"... a vampire who has bitten {MainManager.Instance.NumberOfPeopleBitten} people without getting caught by the guards.");
                    }
                    else
                    {
                        lines.Add($"... a vampire who has bitten one person without getting caught by the guards.");
                    }

                    continue;
                }

                lines.Add($"... a {condition}.");
            }
        }

        // Dog
        if (MainManager.Instance.HasBefriendedDog)
        {
            lines.Add("A speaking dog became your best and only friend.");
        }

        lines.Add("The King's Crown is no more.");

        return lines.ToArray();
    }
}
