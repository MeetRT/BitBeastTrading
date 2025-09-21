namespace EquityEdge.Web.Services;

public class WatchlistStore
{
    private readonly Dictionary<string, List<string>> _userWatchlists = new();

    public List<string> GetWatchlist(string userEmail)
    {
        _userWatchlists.TryGetValue(userEmail, out var watchlist);
        return watchlist ?? new List<string>();
    }

    public void SaveWatchlist(string userEmail, List<string> symbols)
    {
        _userWatchlists[userEmail] = symbols.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }
}