namespace AnimalHaus.Pigpen.Modules;

public sealed class FeedingModule
{
    public int FeedStock { get; private set; }

    public int HungerScore { get; private set; } = 45;

    public bool NeedsFeed => FeedStock <= 0;

    public void ReceiveFeed(int quantity) => FeedStock += quantity;

    public bool AdvanceTick()
    {
        if (FeedStock > 0)
        {
            FeedStock--;
            HungerScore = Math.Max(0, HungerScore - 20);
            return true;
        }

        HungerScore = Math.Min(100, HungerScore + 10);
        return false;
    }
}
