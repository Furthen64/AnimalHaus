namespace AnimalHaus.Barn.Modules;

public sealed class InventoryModule
{
    public int FeedUnits { get; private set; } = 6;

    public bool CanAllocateFeed(int quantity) => quantity > 0 && quantity <= FeedUnits;

    public bool TryAllocateFeed(int quantity)
    {
        if (!CanAllocateFeed(quantity))
        {
            return false;
        }

        FeedUnits -= quantity;
        return true;
    }
}
