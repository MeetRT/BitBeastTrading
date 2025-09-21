using EquityEdge.Web.Services;

namespace EquityEdge.Web.Services;

public class BacktestService
{
    private readonly KiteClient _kiteClient;

    public BacktestService(KiteClient kiteClient)
    {
        _kiteClient = kiteClient;
    }

    public async Task<List<Historical>> GetBars(string accessToken, uint token, string interval, DateTime from, DateTime to)
    {
        try
        {
            var kite = _kiteClient.Api(accessToken);
            var data = kite.GetHistoricalData(token.ToString(), from, to, interval);
            return data;
        }
        catch
        {
            return new List<Historical>();
        }
    }
}