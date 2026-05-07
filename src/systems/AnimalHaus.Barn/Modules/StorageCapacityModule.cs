namespace AnimalHaus.Barn.Modules;

public sealed class StorageCapacityModule
{
    public int Capacity { get; } = 24;

    public bool CanAllocate(int quantity) => quantity <= Capacity;
}
