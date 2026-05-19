using AnimalHaus.Pigpen;

namespace AnimalHaus.Pigpen.Modules;

public sealed class FeedingModule
{
    private readonly int _feedPerTick;
    private readonly int _hungerDecreasePerFeed;
    private readonly int _hungerIncreasePerTick;

    public int FeedStock { get; private set; }

    public int HungerScore { get; private set; }

    public bool NeedsFeed => FeedStock <= 0;

    public FeedingModule(PigpenOptions options)
    {
        FeedStock = options.InitialFeedStock;
        HungerScore = options.InitialHungerScore;
        _feedPerTick = options.FeedPerTick;
        _hungerDecreasePerFeed = options.HungerDecreasePerFeed;
        _hungerIncreasePerTick = options.HungerIncreasePerTick;
    }

    public void ReceiveFeed(int quantity) => FeedStock += quantity;

    public bool AdvanceTick()
    {
        if (FeedStock > 0)
        {
            FeedStock -= _feedPerTick;
            HungerScore = Math.Max(0, HungerScore - _hungerDecreasePerFeed);
            return true;
        }

        HungerScore = Math.Min(100, HungerScore + _hungerIncreasePerTick);
        return false;
    }
}
