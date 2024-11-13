using Microsoft.Extensions.Options;

namespace DepthCharts;

public class NflDepthChartScraperHttpClientHelper(HttpClient httpClient, IOptions<DepthChartsUrlsSettings> options, ILogger<NflDepthChartScraperHttpClientHelper> logger)
{
    private readonly DepthChartsUrlsSettings _depthChartsUrlsSettings = options.Value;
    
    
    public async Task<string> GetTeamDepthChartHtml(string teamName)
    {
        var url = $"{_depthChartsUrlsSettings?.NflUrl}/pfdepthchart/{teamName}";
        logger.LogDebug($"{nameof(GetTeamDepthChartHtml)} called on url:{url}");
        return await httpClient.GetStringAsync(url);
    }

    public async Task<string> GetTeamDepthChartCodesHtml()
    {
        var url = $"{_depthChartsUrlsSettings?.NflUrl}/pfdepthcharts.aspx";
        logger.LogDebug($"{nameof(GetTeamDepthChartCodesHtml)} called on url:{url}");
        return await httpClient.GetStringAsync(url);
    }
}