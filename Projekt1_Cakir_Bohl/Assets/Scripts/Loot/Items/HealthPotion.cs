using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : HealingItem, IConsumable
{
    public List<string> UseItem()
    {
        List<string> lines = new List<string>();
        _initialAmount = PlayerManager.Instance.HealthPoints;

        if(_initialAmount == PlayerManager.Instance.GetStartingHealth())
        {
            lines.Add("You are already at full health.");
            return lines;
        }

        InventoryManager.Instance.ManageInventory(this, 1, false);
        
        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((_initialAmount + _healingAmount) > PlayerManager.Instance.GetStartingHealth())
        {           
            _healingAmount = PlayerManager.Instance.GetStartingHealth() - _initialAmount;
        }

        PlayerManager.Instance.HealthPoints += _healingAmount;

        lines.Add($"You have recovered {_healingAmount} health!"); 
        lines.Add("You throw away the flask. Littering is cool. Only weirdos care about nature.");
        lines.Add("For a moment you wonder what you should do with the empty flask now. You throw it away.");

        onHeal?.Invoke(true, _initialAmount, _healingAmount);

        return lines;
    }
}
