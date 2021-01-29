public static class ResourceTracker
{
    public delegate void OnMoneyChangeDelegate(int oldVal, int newVal);
    public static event OnMoneyChangeDelegate OnMoneyChange;
    private static int money = 0;
    public static int Money { get => money; set { OnMoneyChange?.Invoke(money, value); money = value; } }
}
