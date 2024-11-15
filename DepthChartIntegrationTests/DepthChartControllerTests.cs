using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DepthCharts;
using DepthCharts.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace DepthChartIntegrationTests;

public class
    DepthChartControllerTests : IClassFixture<WebApplicationFactory<Program>> // Replace Startup with your actual Startup class
{
    private readonly HttpClient _client;

    public DepthChartControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<HttpResponseMessage> AddPlayer(string sport, string team, string position, int playerNumber,
        string playerName, int positionDepth)
    {
        // Arrange
        var playerDto = new PlayerDto
            { Name = playerName, Position = position, Number = playerNumber }; // Adjust properties as needed
        var content = new StringContent(JsonConvert.SerializeObject(playerDto), Encoding.UTF8, "application/json");

        // Act
        var response =
            await _client.PostAsync($"/api/DepthChart/{sport}/{team}/player/{positionDepth}",
                content); // Adjust URL as needed
        return response;
    }

    [Fact]
    public async Task AddPlayerToDepthChart_ShouldReturnCreated_WhenValidDataIsProvided()
    {
        var response = await AddPlayer("NFL", "TEAMA", "QB", 1, "Player 1", 1);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 201
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.Contains("TEAMA", response.Headers.Location.ToString(), StringComparison.InvariantCulture);
    }

    [Fact]
    public async Task RemovePlayerFromDepthChart_ShouldReturnNoContent_WhenValidDataIsProvided()
    {
        // Arrange
        var playerName = "Player1"; // Ensure this player exists in the depth chart
        var sport = "NFL";
        var team = "TEAMA";
        var position = "QB";
        await AddPlayer(sport, team, position, 1, playerName, 1);

        // Act
        var response = await _client.DeleteAsync($"/api/DepthChart/{sport}/{team}/{position}/{playerName}");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 204
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetBackups_ShouldReturnOk_WhenValidDataIsProvided()
    {
        // Arrange
        var sport = "NFL";
        var team = "TEAMA";
        var position = "QB";
        var playerName1 = "Player1"; // Ensure this player exists in the depth chart
        var playerName2 = "Player2"; // Ensure this player exists in the depth chart
        
        await AddPlayer(sport, team, position, 1, playerName1, 1);
        await AddPlayer(sport, team, position, 1, playerName2, 2);
        // Act
        var response = await _client.GetAsync($"/api/DepthChart/{sport}/{team}/backups/{position}/{playerName1}");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200
        var content = await response.Content.ReadAsStringAsync();
        var backups = JsonConvert.DeserializeObject<List<BackupPlayersDto>>(content);
        Assert.NotNull(backups);
        Assert.Equal(2, backups.Count);
        Assert.Equal(playerName2, backups[0].PlayerName);
    }

    [Fact]
    public async Task GetFullDepthChart_ShouldReturnOk_WhenValidDataIsProvided()
    {
        // Arrange
        var sport = "NFL";
        var team = "TEAMA";
        
        await AddPlayer(sport, team, "QB", 1, "playerName1", 2);

        // Act
        var response = await _client.GetAsync($"/api/DepthChart/{sport}/{team}/fullDepthChart");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200
        var content = await response.Content.ReadAsStringAsync();
        var depthChart = JsonConvert.DeserializeObject<FullDepthChartDto>(content);
        Assert.NotNull(depthChart);
        Assert.Equal("NFL", depthChart.Sport);
        Assert.Single(depthChart.Teams);
        Assert.Equal("TEAMA", depthChart.Teams[0].TeamName);
        Assert.Single(depthChart.Teams[0].Positions);
        Assert.Single(depthChart.Teams[0].Positions[0].Players);
        Assert.Equal("playerName1", depthChart.Teams[0].Positions[0].Players[0].PlayerName);
    }

    [Fact]
    public async Task ScrapeTeamDepthChart_ShouldReturnBadRequest_WhenInValidDataIsProvided()
    {
        // Arrange
        var sport = "NFL";
        var team = "TEAMA";

        // Act
        var response = await _client.GetAsync($"/api/DepthChart/{sport}/{team}/scrape");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); 
    }

    // Add more tests as needed for other endpoints
}