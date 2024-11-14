using DepthCharts.Models;

namespace DepthCharts;

public interface IDepthChartScraper
{
    public abstract string Sport { get; }
    Task<DepthChartModel> GetTeamDepthChart(string teamName);
    DepthChartModel GetTeamDepthChart(string teamName, string teamUrl);
    Task<SortedSet<string>> GetTeamDepthChartCodes();
}
