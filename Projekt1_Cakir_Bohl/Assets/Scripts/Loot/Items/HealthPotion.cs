using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : Item, IConsumable
{
    private int _healingAmount;

    public List<string> UseItem()
    {
        List<string> lines = new List<string>();

        if(PlayerManager.Instance.HealthPoints== GameConfig.PlayerStartingHealth)
        {
            lines.Add("You are already at full health.");
            return lines;
        }

        InventoryManager.Instance.ManageInventory(this, 1, false);
        
        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((PlayerManager.Instance.HealthPoints + _healingAmount) > GameConfig.PlayerStartingHealth)
        {           
            _healingAmount = GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints;
        }

        PlayerManager.Instance.HealthPoints += _healingAmount;

        lines.Add($"You have recovered {_healingAmount} health!"); 
        lines.Add("You throw away the flask. Littering is cool. Only weirdos care about nature.");
        lines.Add("For a moment you wonder what you should do with the empty flask now. You throw it away.");

        return lines;
    }
}
