using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace DepthCharts.tests;

[Collection("Directory collection")]
public class NflDepthChartScraperTest : IDisposable
{
    [Fact]
    public void TestOurLadsDepthChartTampaBayScraper()
    {
        var html = File.ReadAllText("./data/OurladsNFLPrinter-FriendlyDepthChartsTampaBayBuccaneers.html");
        
        // Create the mocks for the dependencies
        var mockHttpClientHelper = new Mock<NflDepthChartScraperHttpClientHelper>(MockBehavior.Strict, new HttpClient(), new Mock<IOptions<DepthChartsUrlsSettings>>().Object, new Mock<ILogger<NflDepthChartScraperHttpClientHelper>>().Object);
        var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
        
        var scraper = new NflDepthChartScraper(mockHttpClientHelper.Object, mockLogger.Object);

        var depthChart = scraper.GetTeamDepthChart("TB", html);
        
        Assert.NotNull(depthChart);
        Assert.NotNull(depthChart.ParsedDepthChartData);
        Assert.NotEmpty(depthChart.ParsedDepthChartData);
        Assert.Equal(37, depthChart.ParsedDepthChartData.Count);
    }
    
    [Fact]
    public void TestOurLadsDepthChartCodesScraper()
    {
        var html = File.ReadAllText("./data/OurladsNFLPrinter-FriendlyDepthChartsAll.html");
        
        // Create the mocks for the dependencies
        var mockHttpClientHelper = new Mock<NflDepthChartScraperHttpClientHelper>(MockBehavior.Strict, new HttpClient(), new Mock<IOptions<DepthChartsUrlsSettings>>().Object, new Mock<ILogger<NflDepthChartScraperHttpClientHelper>>().Object);
        var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
        
        var scraper = new NflDepthChartScraper(mockHttpClientHelper.Object, mockLogger.Object);

        var teamCodes = scraper.GetTeamDepthChartCodes(html);
        
        Assert.NotEmpty(teamCodes);
        Assert.Equal(NflDepthChartScraper.Nofltteams, teamCodes.Count);
    }

    public void Dispose()
    {
    }
}