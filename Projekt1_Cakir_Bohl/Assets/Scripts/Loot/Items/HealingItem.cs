public class HealingItem : Item
{
    protected int _healingAmount;
    protected int _initialAmount;
    public delegate void OnHeal(bool isHealthHeal, int initialAmount, int healingAmount);
    public static OnHeal onHeal;
}
