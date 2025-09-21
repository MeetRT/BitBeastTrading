using EquityEdge.Web.Services;

namespace EquityEdge.Web.Services;

public class LiveTickerService
{
    private Ticker? _ticker;
    private readonly BarBuilderService _barBuilder;
    private readonly ILogger<LiveTickerService> _logger;

    public event Action<long, string>? OnTick;
    public event Action<long, MinuteBar>? OnMinuteBar;

    public LiveTickerService(BarBuilderService barBuilder, ILogger<LiveTickerService> logger)
    {
        _barBuilder = barBuilder;
        _logger = logger;
    }

    public void Start(string apiKey, string accessToken, uint[] tokens)
    {
        try
        {
            _ticker = new Ticker(apiKey, accessToken);
            
            _ticker.OnTick += (tick) =>
            {
                try
                {
                    var token = (long)tick.InstrumentToken;
                    var price = tick.LastPrice;
                    var volume = (long)tick.Volume;
                    var timestamp = DateTime.UtcNow;

                    var tickLine = $"{timestamp:HH:mm:ss} - {token}: ₹{price:F2} (Vol: {volume:N0})";
                    OnTick?.Invoke(token, tickLine);

                    var completedBar = _barBuilder.ProcessTick(token, price, volume, timestamp);
                    if (completedBar != null)
                    {
                        OnMinuteBar?.Invoke(token, completedBar);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tick");
                }
            };

            _ticker.OnConnect += () =>
            {
                _logger.LogInformation("Ticker connected");
                _ticker.Subscribe(tokens);
                _ticker.SetMode(Constants.MODE_QUOTE, tokens);
            };

            _ticker.OnError += (exception) =>
            {
                _logger.LogError(exception, "Ticker error");
            };

            _ticker.OnClose += () =>
            {
                _logger.LogInformation("Ticker disconnected");
            };

            _ticker.Connect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ticker");
        }
    }

    public void Stop()
    {
        _ticker?.Close();
        _ticker = null;
    }
}