using UnityEngine;

public static class DiceUtil
{
    public static int D10()
    {
        return Random.Range(1, 11);
    }

    public static int D6()
    {
        return Random.Range(1, 7);
    }

    public static int D4()
    {
        return Random.Range(1, 5);
    }
}
