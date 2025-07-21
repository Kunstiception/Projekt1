using System.Collections.Generic;

public class EgoRing : Equipment, IEquipable
{
    public List<string> EquipItem(bool isEuip)
    {
        List<string> lines = new List<string>();

        if (isEuip)
        {
            PlayerManager.Instance.EgoPointsModifier += GameConfig.AdditionalEgoPoints;
            //PlayerManager.Instance.EgoPoints += GameConfig.AdditionalEgoPoints;

            lines.Add("The ring slips on your finger easily.");
            lines.Add("You feel better.");
            lines.Add("Better than all the others.");

            return lines;
        }
        else
        {
            PlayerManager.Instance.EgoPointsModifier -= GameConfig.AdditionalEgoPoints;
            //PlayerManager.Instance.EgoPoints -= GameConfig.AdditionalEgoPoints;

            PlayerManager.Instance.SetStatsAfterChange();

            lines.Add($"You put {Name} back into your pocket.");

            return lines;
        }
    }
}
