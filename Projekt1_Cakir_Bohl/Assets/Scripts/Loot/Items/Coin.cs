public class Coin : Item, IConsumable
{
    public string[] UseItem()
    {
        string[] lines = {"You hold the coin in your hand. It shines brightly.", "It is weirdly light... too light?"};

        return lines;
    }
}
