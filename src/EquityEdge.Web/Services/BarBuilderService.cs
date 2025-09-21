namespace EquityEdge.Web.Services;

public class MinuteBar
{
    public long Token { get; set; }
    public DateTime Time { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long Volume { get; set; }
}

public class BarBuilderService
{
    private readonly Dictionary<long, MinuteBar> _currentBars = new();

    public MinuteBar? ProcessTick(long token, double price, long volume, DateTime timestamp)
    {
        var minute = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 
            timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc);

        if (!_currentBars.TryGetValue(token, out var bar) || bar.Time != minute)
        {
            var completedBar = bar;
            
            _currentBars[token] = new MinuteBar
            {
                Token = token,
                Time = minute,
                Open = price,
                High = price,
                Low = price,
                Close = price,
                Volume = volume
            };

            return completedBar;
        }

        bar.High = Math.Max(bar.High, price);
        bar.Low = Math.Min(bar.Low, price);
        bar.Close = price;
        bar.Volume += volume;

        return null;
    }

    public MinuteBar? GetCurrentBar(long token)
    {
        _currentBars.TryGetValue(token, out var bar);
        return bar;
    }
}