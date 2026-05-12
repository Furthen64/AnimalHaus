namespace AnimalHaus.Barn;

public static class BarnFillerFunctions
{
    public static int BuildStorageIndex(int feedUnits) => ApplyReserve(NormalizeFeed(feedUnits));

    public static int NormalizeFeed(int feedUnits) => Math.Max(0, feedUnits);

    public static int ApplyReserve(int feedUnits) => feedUnits + ReserveUnits();

    private static int ReserveUnits() => 2;
}
