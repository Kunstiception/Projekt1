using System.Collections.Generic;

public class HealthRing : Equipment, IEquipable
{
    public List<string> EquipItem(bool isEquip)
    {
        List<string> lines = new List<string>();

        if (isEquip)
        {

            PlayerManager.Instance.HealthPointsModifier += GameConfig.AdditionalHealthPoints;

            lines.Add("The ring slips on your finger easily.");
            lines.Add("You feel good.");
            lines.Add("Stronger.");
            lines.Add("More alive!");

            return lines;
        }
        else
        {
            lines.Add($"You put {Name} back into your pocket.");
            
            return lines;
        }

    }
}
