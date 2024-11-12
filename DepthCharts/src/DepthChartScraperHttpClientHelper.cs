using Microsoft.Extensions.Options;

namespace DepthCharts;

public class DepthChartScraperHttpClientHelper(HttpClient httpClient, IOptions<DepthChartsUrlsSettings> options, ILogger<DepthChartScraperHttpClientHelper> logger)
{
    private readonly DepthChartsUrlsSettings _depthChartsUrlsSettings = options.Value;
    
    
    public async Task<string> GetTeamDepthChartHtml(string teamName)
    {
        var url = $"{_depthChartsUrlsSettings?.NflUrl}/{teamName}";
        logger.LogDebug($"{nameof(GetTeamDepthChartHtml)} called on url:{url}");
        return await httpClient.GetStringAsync(url);
    }
    
}