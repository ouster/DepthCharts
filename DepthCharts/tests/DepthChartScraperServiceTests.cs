using DepthCharts.Models;
using Moq;
using Xunit;

namespace DepthCharts.Tests
{
    public class DepthChartScraperServiceTests
    {
        [Fact]
        public void Constructor_ShouldThrowException_WhenNoScrapersAreRegistered()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DepthChartScraperService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                new DepthChartScraperService(Enumerable.Empty<IDepthChartScraper>(), mockServiceProvider.Object,
                    mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldNotThrowException_WhenScrapersAreRegistered()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DepthChartScraperService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockScraper = new Mock<IDepthChartScraper>();
            mockScraper.Setup(scraper => scraper.Sport).Returns("Basketball");

            var scrapers = new List<IDepthChartScraper> { mockScraper.Object };

            // Act & Assert
            var service = new DepthChartScraperService(scrapers, mockServiceProvider.Object, mockLogger.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public async Task Scrape_ShouldReturnDepthChart_WhenValidSportAndTeamAreProvided()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DepthChartScraperService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockScraper = new Mock<IDepthChartScraper>();
            mockScraper.Setup(scraper => scraper.Sport).Returns("Basketball");
            mockScraper.Setup(scraper => scraper.GetTeamDepthChart(It.IsAny<string>()))
                .ReturnsAsync(new DepthChartModel());

            var scrapers = new List<IDepthChartScraper> { mockScraper.Object };
            var service = new DepthChartScraperService(scrapers, mockServiceProvider.Object, mockLogger.Object);

            // Act
            var result = await service.Scrape("Basketball", "Lakers");

            // Assert
            Assert.NotNull(result);
            mockScraper.Verify(scraper => scraper.GetTeamDepthChart("Lakers"), Times.Once);
        }

        [Fact]
        public async Task ScrapeTeamCodes_ShouldReturnTeamCodes_WhenValidSportIsProvided()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DepthChartScraperService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockScraper = new Mock<IDepthChartScraper>();
            mockScraper.Setup(scraper => scraper.Sport).Returns("Basketball");
            mockScraper.Setup(scraper => scraper.GetTeamDepthChartCodes()).ReturnsAsync([]);

            var scrapers = new List<IDepthChartScraper> { mockScraper.Object };
            var service = new DepthChartScraperService(scrapers, mockServiceProvider.Object, mockLogger.Object);

            // Act
            var result = await service.ScrapeTeamCodes("Basketball");

            // Assert
            Assert.NotNull(result);
            mockScraper.Verify(scraper => scraper.GetTeamDepthChartCodes(), Times.Once);
        }

        [Fact]
        public async Task GetScraperForSport_ShouldThrowArgumentException_WhenSportIsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DepthChartScraperService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockScraper = new Mock<IDepthChartScraper>();
            mockScraper.Setup(scraper => scraper.Sport).Returns("Basketball");

            var scrapers = new List<IDepthChartScraper> { mockScraper.Object };
            var service = new DepthChartScraperService(scrapers, mockServiceProvider.Object, mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.Scrape("Football", "Team"));
        }
    }
}