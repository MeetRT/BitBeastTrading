namespace EquityEdge.Web.Services;

public class LiveInterop
{
    private readonly LiveTickerBridge _bridge;
    private readonly IConfiguration _configuration;

    public LiveInterop(LiveTickerBridge bridge, IConfiguration configuration)
    {
        _bridge = bridge;
        _configuration = configuration;
    }

    public void Start(string accessToken, uint[] tokens, Func<long, string, Task> onTick, Func<long, MinuteBar, Task> onMinute)
    {
        _bridge.RegisterCallbacks(onTick, onMinute);
        var apiKey = _configuration["Kite:ApiKey"] ?? "";
        _bridge.StartFeed(apiKey, accessToken, tokens);
    }

    public void Stop()
    {
        _bridge.StopFeed();
    }
}