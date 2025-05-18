using UnityEngine;

[CreateAssetMenu(fileName = "New InsultsAndValues", menuName = "Scriptable Object/InsultsAndValues")]
public class InsultLines : ScriptableObject
{
    public string[] Insults;
    public int[] Values;
    public string[] AnswersWhenHit;
    public string[] AnswersWhenResisted;
}
