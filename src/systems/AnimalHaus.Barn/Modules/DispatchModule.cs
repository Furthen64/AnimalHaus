namespace AnimalHaus.Barn.Modules;

public sealed class DispatchModule
{
    public string CreateTaskName(int tick) => $"haul-feed-{tick}";
}
