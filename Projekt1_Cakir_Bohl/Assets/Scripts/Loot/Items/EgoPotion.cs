
using System.Collections.Generic;
using UnityEngine;

public class EgoPotion : Item, IConsumable
{
    private int _healingAmount;

    public List<string> UseItem()
    {
        if(PlayerManager.Instance.EgoPoints == GameConfig.PlayerStartingEgo)
        {
            _lines.Add("You are already at full ego.");
            return _lines;
        }

        InventoryManager.Instance.ManageInventory(this, 1, false);

        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((PlayerManager.Instance.EgoPoints + _healingAmount) > GameConfig.PlayerStartingEgo)
        {
            _healingAmount = GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints;
        }

        PlayerManager.Instance.EgoPoints += _healingAmount;

        _lines.Add($"You have recovered {_healingAmount} ego!"); 
        _lines.Add("You throw away the flask. Littering is cool.");
        _lines.Add("Only weirdos care about nature."); 
        _lines.Add("For a moment you wonder what you should do with the empty flask now. You throw it away.");

        return _lines;
    }
}
