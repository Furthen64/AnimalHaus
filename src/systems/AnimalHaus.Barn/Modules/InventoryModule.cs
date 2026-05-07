namespace AnimalHaus.Barn.Modules;

public sealed class InventoryModule
{
    public int FeedUnits { get; private set; } = 6;

    public bool TryAllocateFeed(int quantity)
    {
        if (quantity <= 0 || quantity > FeedUnits)
        {
            return false;
        }

        FeedUnits -= quantity;
        return true;
    }
}
