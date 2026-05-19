using AnimalHaus.Barn;

namespace AnimalHaus.Barn.Modules;

public sealed class StorageCapacityModule
{
    public int Capacity { get; }

    public StorageCapacityModule(BarnOptions options)
    {
        Capacity = options.StorageCapacity;
    }

    public bool CanAllocate(int quantity) => quantity <= Capacity;
}
