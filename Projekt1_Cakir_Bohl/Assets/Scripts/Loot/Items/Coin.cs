using System.Collections.Generic;

public class Coin : Item, IConsumable
{
    public List<string> UseItem()
    {
        List<string> lines = new List<string>
        {
            "You hold the coin in your hand. It shines brightly.",
            "It is weirdly light... too light?"
        };

        return lines;
    }
}
