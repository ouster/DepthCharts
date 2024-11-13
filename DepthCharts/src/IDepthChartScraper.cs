namespace DepthCharts;

public interface IDepthChartScraper
{
    public abstract string Sport { get; }
    Task<ParsedDepthChart> GetTeamDepthChart(string teamName);
    ParsedDepthChart GetTeamDepthChart(string teamName, string teamUrl);
    Task<SortedSet<string>> GetTeamDepthChartCodes();
}
