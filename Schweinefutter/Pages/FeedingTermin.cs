namespace Schweinefutter.Pages;

/// <summary>
/// Objekt eines Fresstermines eines Schweins. Kann die Zeit zwischen Anfang und Ende mitteln.
/// </summary>
public class FeedingTermin
{
    public DateTime FeedStart { set; get; }
    public DateTime FeedStop { set; get; }
    public float KGAmount { set; get; }

    public DateTime GetAverageTime()
    {
        // Long >> 1 --> Long / 2, aber deutlich schneller weil weniger Zyklen (der Code wird leider nicht optimiert)
        return new DateTime((FeedStart.Ticks + FeedStop.Ticks) >> 1);
       // return new DateTime((FeedStart.Ticks + FeedStop.Ticks)/2); Falls bitshift unerwünscht.
    }
}