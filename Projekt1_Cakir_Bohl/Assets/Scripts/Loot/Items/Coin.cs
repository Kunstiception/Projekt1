using System.Collections.Generic;
using UnityEngine;

public class Coin : Item, IUsable
{
    [TextArea]
    public string LookAtText;
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
