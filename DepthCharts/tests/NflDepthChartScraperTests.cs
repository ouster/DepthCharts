using Moq;
using Xunit;

namespace DepthCharts.Tests
{
    public class NflDepthChartScraperTests
    {
        [Fact]
        public async Task GetTeamDepthChart_ShouldReturnDepthChart_WhenValidHtmlIsProvided()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
            var mockHttpHelper = new Mock<INflDepthChartScraperHttpClientHelper>();
            mockHttpHelper
                .Setup(helper => helper.GetTeamDepthChartHtml(It.IsAny<string>()))
                .ReturnsAsync("<html><table id='gvChart'><tr><td>QB</td><td>1</td><td>Player 1</td></tr></table></html>");
            var scraper = new NflDepthChartScraper(mockHttpHelper.Object, mockLogger.Object);

            // Act
            var result = await scraper.GetTeamDepthChart("TeamName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TeamName", result.Team);
            Assert.Single(result.TeamDepthChartGroupList ?? throw new InvalidOperationException());
            Assert.Equal("QB", result.TeamDepthChartGroupList[0].Position);
        }

        [Fact]
        public async Task GetTeamDepthChart_ShouldThrowDepthChartParseException_WhenTableNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
            var mockHttpHelper = new Mock<INflDepthChartScraperHttpClientHelper>();
            mockHttpHelper
                .Setup(helper => helper.GetTeamDepthChartHtml(It.IsAny<string>()))
                .ReturnsAsync("<html><table></table></html>");

            var scraper = new NflDepthChartScraper(mockHttpHelper.Object, mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DepthChartParseException>(() => scraper.GetTeamDepthChart("TeamName"));
            Assert.Equal("Table not found on the page.", exception.Message);
        }

        [Fact]
        public void RepairData_ShouldAdjustList_WhenOddNumberOfPlayers()
        {
            // Arrange
            var input = new List<string> { "1", "Player 1", "2", "Player 2", "3" };

            // Act
            var result = NflDepthChartScraper.RepairData(input);

            // Assert
            Assert.Equal(6, result.Count);
            Assert.Equal("2", result[2]);
            Assert.Equal("Unnamed", result[5]);
        }

        [Fact]
        public async Task GetTeamDepthChartCodes_ShouldReturnSortedCodes_WhenValidHtmlIsProvided()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
           
            var mockHttpHelper = new Mock<INflDepthChartScraperHttpClientHelper>();
            mockHttpHelper
                .Setup(helper => helper.GetTeamDepthChartCodesHtml())
                .ReturnsAsync("<html><table id='gvChart'>" +
                              "<tr><td>Team1</td><td>Player1</td><td>Position1</td></tr>" +
                              "<tr><td>Team2</td><td>Player2</td><td>Position2</td></tr>" +
                              "<tr><td>Team3</td><td>Player3</td><td>Position3</td></tr>" +
                              "</table></html>");
            
            var scraper = new NflDepthChartScraper(mockHttpHelper.Object, mockLogger.Object);

            // Act
            var result = await scraper.GetTeamDepthChartCodes();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains("Team1", result);
            Assert.Contains("Team2", result);
            Assert.Contains("Team3", result);
        }

        [Fact]
        public async Task GetTeamDepthChartCodes_ShouldThrowDepthChartParseException_WhenTableNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NflDepthChartScraper>>();
            
            var mockHttpHelper = new Mock<INflDepthChartScraperHttpClientHelper>();
            mockHttpHelper
                .Setup(helper => helper.GetTeamDepthChartCodesHtml())
                .ReturnsAsync("<html><table></table></html>");

            var scraper = new NflDepthChartScraper(mockHttpHelper.Object, mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DepthChartParseException>(() => scraper.GetTeamDepthChartCodes());
            Assert.Equal("Table not found on the page.", exception.Message);
        }
    }
}
