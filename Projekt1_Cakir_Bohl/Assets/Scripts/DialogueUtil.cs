using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class DialogueUtil
{
    public static IEnumerator DisplayTextOverTime(string line, TextMeshProUGUI uiElement)
    {
        char[] chars = line.ToCharArray();

        string currentString = "";

        for(int i = 0; i < chars.Length; i++)
        {
            currentString = currentString + chars[i];
            uiElement.text = currentString;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
    }

    public static void ShowFullLine(string line, TextMeshProUGUI uiElement)
    {
        uiElement.text = line;
    }
}
