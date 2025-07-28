using System.Collections;
using TMPro;
using UnityEngine;

public static class DialogueUtil
{
    public static IEnumerator DisplayTextOverTime(string line, TextMeshProUGUI uiElement, TextMeshProUGUI promptSkip, TextMeshProUGUI promptContinue)
    {
        char[] chars = line.ToCharArray();

        uiElement.text = "";
        string currentString = "";

        if (!PlayerManager.Instance.IsAuto)
        {
            promptSkip.enabled = true;
        }

        while (uiElement.text.Length < chars.Length)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                currentString = currentString + chars[i];
                uiElement.text = currentString;
                yield return new WaitForSeconds(GameConfig.TimeBetweenChars);
            }
        }

        if (!PlayerManager.Instance.IsAuto)
        {
            promptSkip.enabled = false;
        }
    }

    public static IEnumerator WaitForContinue(TextMeshProUGUI promptContinue)
    {      
        yield return new WaitForSeconds(GameConfig.TimeBeforeNextLine);

        promptContinue.enabled = true;

        if (!PlayerManager.Instance.IsAuto)
        {
            while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.RightAlt))
            {
                yield return null;
            }        
        }

        promptContinue.enabled = false;
    }

    public static void ShowFullLine(string line, TextMeshProUGUI uiElement, TextMeshProUGUI promptSkip)
    {
        uiElement.text = line;
        promptSkip.enabled = false;
    }

    public static string CreateCombatLog(Combatant combatant, string verb, string lineEnding)
    {
        string name = combatant.Name != PlayerManager.Instance.Name ? combatant.Name : PlayerManager.Instance.Name;
        string newVerb;

        if (name == PlayerManager.Instance.Name)
        {
            newVerb = ManageVerb(verb);
        }
        else
        {
            newVerb = verb;
        }

        return name + " " + newVerb + " " + lineEnding;
    }

    public static string ManageVerb(string verb)
    {       
        switch (verb)
        {
            case "has":
                return "have";
            case "was":
                return "were";
            case "goes":
                return "go";
            default:
                return verb.TrimEnd('s');
        }
    }

    public static string AddEnding(string line, int count)
    {
        if(count > 1)
        {
            return line + "s";
        }
         return line;
    }
}
