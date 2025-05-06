
using System.Collections.Generic;
using UnityEngine;

public class EgoPotion : HealingItem, IConsumable
{
    public List<string> UseItem()
    {
        List<string> lines = new List<string>();
        _initialAmount = PlayerManager.Instance.EgoPoints;
        
        if(_initialAmount == PlayerManager.Instance.GetStartingEgo())
        {
            lines.Add("You are already at full ego.");
            return lines;
        }

        InventoryManager.Instance.ManageInventory(this, 1, false);

        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((PlayerManager.Instance.EgoPoints + _healingAmount) > _initialAmount)
        {
            _healingAmount = PlayerManager.Instance.GetStartingEgo() - _initialAmount;
        }

        PlayerManager.Instance.EgoPoints += _healingAmount;

        lines.Add($"You have recovered {_healingAmount} ego!"); 
        lines.Add("You throw away the flask. Littering is cool.");
        lines.Add("For a moment you wonder what you should do with the empty flask now. You throw it away.");

        onHeal?.Invoke(false, _initialAmount, _healingAmount);

        return lines;
    }
}
