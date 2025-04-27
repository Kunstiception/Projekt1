using UnityEngine;

public class HealthPotion : Item, IConsumable
{
    private int _healingAmount;
    private string[] _lines;

    public string[] UseItem()
    {
        if(PlayerManager.Instance.HealthPoints== GameConfig.PlayerStartingHealth)
        {
            
            //return "You are already at full health.";
        }
        
        _healingAmount = Random.Range(GameConfig.MinimumHeal, GameConfig.MaximumHeal + 1);

        if((PlayerManager.Instance.HealthPoints += _healingAmount) > GameConfig.PlayerStartingHealth)
        {           
            _healingAmount = GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints;
        }

        PlayerManager.Instance.HealthPoints += _healingAmount;

        string[] lines = {$"You have recovered {_healingAmount} health!", "You throw away the flask. Littering is cool. Only weirdos care about nature.", 
            "For a moment you wonder what you should do with the empty flask now. You throw it away."};

        return lines;
    }
}
