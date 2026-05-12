namespace AnimalHaus.Barn;

public static class BarnFillerFunctions
{
    public static int BuildStorageIndex(int feedUnits) => ApplyReserve(NormalizeFeed(feedUnits));

    public static int NormalizeFeed(int feedUnits) => Math.Clamp(feedUnits, 0, int.MaxValue - ReserveUnits());

    public static int ApplyReserve(int feedUnits) => checked(feedUnits + ReserveUnits());

    private static int ReserveUnits() => 2;
}
