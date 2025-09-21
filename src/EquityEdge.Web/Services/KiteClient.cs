using System.Collections.Generic;

namespace EquityEdge.Web.Services;

// Mock implementations for KiteConnect types
public class Kite
{
    public Kite(string apiKey, bool Debug = false) { }
    
    public void SetAccessToken(string accessToken) { }
    
    public UserSession GenerateSession(string requestToken, string apiSecret)
    {
        return new UserSession { AccessToken = "mock_access_token" };
    }
    
    public List<Historical> GetHistoricalData(string instrument, DateTime from, DateTime to, string interval)
    {
        // Return mock data
        var data = new List<Historical>();
        var current = from;
        var random = new Random();
        var basePrice = 2500.0;
        
        while (current < to)
        {
            var open = basePrice + random.NextDouble() * 100 - 50;
            var change = random.NextDouble() * 20 - 10;
            var high = Math.Max(open, open + change) + random.NextDouble() * 10;
            var low = Math.Min(open, open + change) - random.NextDouble() * 10;
            var close = open + change;
            
            data.Add(new Historical
            {
                TimeStamp = current,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = (uint)(random.Next(10000, 100000))
            });
            
            current = current.AddMinutes(interval == "5minute" ? 5 : interval == "15minute" ? 15 : 60);
            if (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday)
                current = current.AddDays(2);
        }
        
        return data;
    }
}

public class UserSession
{
    public string AccessToken { get; set; } = string.Empty;
}

public class Historical
{
    public DateTime TimeStamp { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public uint Volume { get; set; }
}

public class Ticker
{
    public Ticker(string apiKey, string accessToken) { }
    
    public event Action<Tick>? OnTick;
    public event Action? OnConnect;
    public event Action<Exception>? OnError;
    public event Action? OnClose;
    
    public void Connect()
    {
        OnConnect?.Invoke();
        // Start mock tick generation
        Task.Run(async () =>
        {
            var random = new Random();
            while (true)
            {
                await Task.Delay(1000);
                OnTick?.Invoke(new Tick
                {
                    InstrumentToken = 884737,
                    LastPrice = 2500 + random.NextDouble() * 100 - 50,
                    Volume = (uint)random.Next(1000, 10000)
                });
            }
        });
    }
    
    public void Subscribe(uint[] tokens) { }
    public void SetMode(string mode, uint[] tokens) { }
    public void Close() { OnClose?.Invoke(); }
}

public class Tick
{
    public uint InstrumentToken { get; set; }
    public double LastPrice { get; set; }
    public uint Volume { get; set; }
}

public static class Constants
{
    public const string MODE_QUOTE = "quote";
}

public class KiteClient
{
    private readonly IConfiguration _configuration;

    public KiteClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetLoginUrl(string state)
    {
        var apiKey = _configuration["Kite:ApiKey"];
        var redirectUrl = _configuration["Kite:RedirectUrl"];
        return $"https://kite.trade/connect/login?api_key={apiKey}&state={state}&redirect_url={Uri.EscapeDataString(redirectUrl)}";
    }

    public async Task<string> ExchangeRequestToken(string requestToken)
    {
        var apiKey = _configuration["Kite:ApiKey"];
        var apiSecret = _configuration["Kite:ApiSecret"];
        
        var kite = new Kite(apiKey, Debug: false);
        var user = kite.GenerateSession(requestToken, apiSecret);
        
        return user.AccessToken;
    }

    public Kite Api(string accessToken)
    {
        var apiKey = _configuration["Kite:ApiKey"];
        var kite = new Kite(apiKey, Debug: false);
        kite.SetAccessToken(accessToken);
        return kite;
    }
}