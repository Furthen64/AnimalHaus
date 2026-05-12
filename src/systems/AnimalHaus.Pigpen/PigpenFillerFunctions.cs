namespace AnimalHaus.Pigpen;

public static class PigpenFillerFunctions
{
    public static int BuildComfortIndex(int baseValue) => AddBonus(Normalize(baseValue));

    public static int Normalize(int value) => Math.Clamp(value, 0, 100);

    public static int AddBonus(int value) => Math.Min(100, value + Bonus());

    private static int Bonus() => 5;
}
