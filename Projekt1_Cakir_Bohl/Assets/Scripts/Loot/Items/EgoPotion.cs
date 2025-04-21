
using UnityEngine;

public class EgoPotion : Item, IConsumable
{
    private int _healingAmount;

    public string[] UseItem()
    {
        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((PlayerManager.Instance.EgoPoints += _healingAmount) > GameConfig.PlayerStartingEgo)
        {
            _healingAmount = GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints;
        }

        PlayerManager.Instance.EgoPoints += _healingAmount;

        string[] lines = {$"You have recovered {_healingAmount} ego!", "You throw away the flask. Littering is cool. Only weirdos care about nature.", 
            "For a moment you wonder what you should do with the empty flask now. You throw it away."};

        return lines;
    }
}
