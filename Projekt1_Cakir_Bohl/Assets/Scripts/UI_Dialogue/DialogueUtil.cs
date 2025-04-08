using System;
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

        if (promptSkip)
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

        if (promptContinue == null && promptSkip == null)
        {
            yield break;
        }

        promptSkip.enabled = false;
        //promptContinue.enabled = true;
    }

    public static IEnumerator WaitForContinue(TextMeshProUGUI promptContinue)
    {      
        yield return new WaitForSeconds(GameConfig.TimeBeforeNextLine);

        promptContinue.enabled = true;

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        promptContinue.enabled = false;
    }

    public static void ShowFullLine(string line, TextMeshProUGUI uiElement, TextMeshProUGUI promptSkip)
    {
        uiElement.text = line;
        promptSkip.enabled = false;
    }
}
