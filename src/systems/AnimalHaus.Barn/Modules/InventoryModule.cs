using AnimalHaus.Barn;

namespace AnimalHaus.Barn.Modules;

public sealed class InventoryModule
{
    public int FeedUnits { get; private set; }

    public InventoryModule(BarnOptions options)
    {
        FeedUnits = options.InitialFeedUnits;
    }

    public bool TryAllocateFeed(int quantity)
    {
        if (quantity <= 0 || quantity > FeedUnits)
        {
            return false;
        }

        FeedUnits -= quantity;
        return true;
    }

    public void RestoreFeed(int quantity)
    {
        if (quantity > 0)
        {
            FeedUnits += quantity;
        }
    }
}
