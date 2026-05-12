namespace AnimalHaus.Barn;

public static class BarnFillerFunctions
{
    public static int BuildStorageIndex(int feedUnits) => ApplyReserve(NormalizeFeed(feedUnits));

    public static int NormalizeFeed(int feedUnits) => Math.Clamp(feedUnits, 0, int.MaxValue);

    public static int ApplyReserve(int feedUnits)
    {
        var total = (long)feedUnits + ReserveUnits();
        return total >= int.MaxValue ? int.MaxValue : (int)total;
    }

    private static int ReserveUnits() => 2;
}
