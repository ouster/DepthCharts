
namespace DepthCharts;

public class DepthChartScraperService 
{
    private readonly Dictionary<string, IDepthChartScraper> _scrapers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DepthChartScraperService> _logger;

    public DepthChartScraperService(IEnumerable<IDepthChartScraper> scrapers,
        IServiceProvider serviceProvider,
        ILogger<DepthChartScraperService> logger)
    {
        _serviceProvider = serviceProvider;
        _scrapers = scrapers.ToDictionary(scraper => scraper.Sport); 

        if (!_scrapers.Any())
            throw new InvalidOperationException("No scrapers were registered.");

        _logger = logger;
    }
    
    public IDepthChartScraper GetScraperForSport(string sport)
    {
        if (_scrapers.TryGetValue(sport, out var forSport))
        {
            return forSport;
        }

        throw new ArgumentException($"No scraper found for team {sport}");
    }

    public async Task<ParsedDepthChart> Scrape(string sport, string teamName)
    {
        var scraper = GetScraperForSport(sport);
        
        return await scraper.GetTeamDepthChart(teamName);
    }
}