using Microsoft.AspNetCore.Components;

namespace EquityEdge.Web.Services;

public class LiveTickerBridge
{
    private readonly LiveTickerService _tickerService;
    private readonly ILogger<LiveTickerBridge> _logger;
    private Func<long, string, Task>? _onTickCallback;
    private Func<long, MinuteBar, Task>? _onMinuteCallback;

    public LiveTickerBridge(LiveTickerService tickerService, ILogger<LiveTickerBridge> logger)
    {
        _tickerService = tickerService;
        _logger = logger;
        
        _tickerService.OnTick += OnTickReceived;
        _tickerService.OnMinuteBar += OnMinuteBarReceived;
    }

    public void RegisterCallbacks(Func<long, string, Task> onTick, Func<long, MinuteBar, Task> onMinute)
    {
        _onTickCallback = onTick;
        _onMinuteCallback = onMinute;
    }

    private async void OnTickReceived(long token, string tickLine)
    {
        if (_onTickCallback != null)
        {
            try
            {
                await _onTickCallback(token, tickLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking tick callback");
            }
        }
    }

    private async void OnMinuteBarReceived(long token, MinuteBar bar)
    {
        if (_onMinuteCallback != null)
        {
            try
            {
                await _onMinuteCallback(token, bar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking minute bar callback");
            }
        }
    }

    public void StartFeed(string apiKey, string accessToken, uint[] tokens)
    {
        _tickerService.Start(apiKey, accessToken, tokens);
    }

    public void StopFeed()
    {
        _tickerService.Stop();
    }
}

public class StartupWiring : IHostedService
{
    private readonly LiveTickerBridge _bridge;

    public StartupWiring(LiveTickerBridge bridge)
    {
        _bridge = bridge;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _bridge.StopFeed();
        return Task.CompletedTask;
    }
}