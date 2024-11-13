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
        var html = File.ReadAllText("./data/OurladsNFLPrinter-FriendlyDepthCharts.html");
        
        // Create the mocks for the dependencies
        var mockHttpClientHelper = new Mock<NflDepthChartScraperHttpClientHelper>(MockBehavior.Strict, new HttpClient(), new Mock<IOptions<DepthChartsUrlsSettings>>().Object, new Mock<ILogger<NflDepthChartScraperHttpClientHelper>>().Object);
        var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
        
        var scraper = new NflDepthChartScraper(mockHttpClientHelper.Object, mockLogger.Object);

        var depthChart = scraper.GetTeamDepthChart("TB", html);
        
        Assert.NotNull(depthChart);
        Assert.NotNull(depthChart.ParsedDepthChartData);
        Assert.NotEmpty(depthChart.ParsedDepthChartData);
        Assert.True(depthChart.ParsedDepthChartData.Count == 37);
    }

    public void Dispose()
    {
    }
}