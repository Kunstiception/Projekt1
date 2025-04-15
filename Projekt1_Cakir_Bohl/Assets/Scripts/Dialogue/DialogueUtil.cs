using System.Collections;
using TMPro;
using UnityEngine;

public static class DialogueUtil
{
    public static IEnumerator DisplayTextOverTime(string line, TextMeshProUGUI uiElement)
    {
        char[] chars = line.ToCharArray();

        string currentString = "";

        for(int i = 0; i < chars.Length; i++)
        {
            if(currentString.Length == chars.Length)
            {
                yield break;
            }

            currentString = currentString + chars[i];
            uiElement.text = currentString;
            yield return new WaitForSeconds(GameConfig.TimeBetweenChars);
        }
    }

    public static void ShowFullLine(string line, TextMeshProUGUI uiElement)
    {
        uiElement.text = line;
    }
}
