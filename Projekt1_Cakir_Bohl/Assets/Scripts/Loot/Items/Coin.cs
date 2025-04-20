using UnityEngine;

public class Coin : MonoBehaviour, IConsumable
{
    public string[] ConsumeAndApplyEffect()
    {
        string[] lines = {"You struggle, but in the end you manage to swallow the coin.", "You don't feel good.", 
            "That was stupid, but at least you don't have to face the consequences right away."};

        return lines;
    }
}
