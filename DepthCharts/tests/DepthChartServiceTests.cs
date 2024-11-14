using AutoMapper;
using DepthCharts.Models;
using Moq;
using Xunit;

namespace DepthCharts.tests;

public class DepthChartServiceTests : IDisposable
{
    private List<(PlayerDto player, int depth)> _players;

    private readonly DepthChartService _depthChartService;

    public DepthChartServiceTests()
    {
        _depthChartService = new DepthChartService();
    }

    private List<(PlayerDto, int)> GetTestPlayers() =>
    [
        (new PlayerDto(12, "Tom Brady", "QB"), 0),
        (new PlayerDto(11, "Blaine Gabbert", "QB"), 1),
        (new PlayerDto(2, "Kyle Trask", "QB"), 2),
        (new PlayerDto(13, "Mike Evans", "WR"), 0),
        (new PlayerDto(1, "Jaelon Darden", "WR"), 1),
        (new PlayerDto(10, "Scott Miller", "WR"), 2)
    ];

    [Fact]
    public void AddPlayerToDepthChart_ShouldAddPlayersAtCorrectPositionsAndReturnExpectedBackups()
    {
        // Arrange
        AddTestPlayers();

        // Assert
        AssertPlayerBackups("QB", "Tom Brady", new[] { ("Blaine Gabbert", 11), ("Kyle Trask", 2) });
        AssertPlayerBackups("WR", "Jaelon Darden", new[] { ("Scott Miller", 10) });
        AssertPlayerBackups("WR", "Mike Evans", new[] { ("Jaelon Darden", 1), ("Scott Miller", 10) });
        AssertPlayerBackups("QB", "Kyle Trask", Array.Empty<(string, int)>());
    }

    private void AddTestPlayers()
    {
        _players = GetTestPlayers();
        foreach (var (player, depth) in _players)
        {
            _depthChartService.AddPlayerToDepthChart("NFL", "TB", player.Position, player.Number, player.Name, depth);
        }
    }

    [Fact]
    public void AddPlayerToDepthChart_ShouldAllowAddSamePlayerToTheDepthChartSameDepth()
    {
        // Arrange
        AddTestPlayers();

        // Act
        _depthChartService.AddPlayerToDepthChart("NFL", "TB", "QB", 12, "Tom Brady", 0);

        // Assert
        AssertPlayerBackups("QB", "Tom Brady", new[] { ("Blaine Gabbert", 11), ("Kyle Trask", 2) });
    }

    [Fact]
    public void AddPlayerToDepthChart_ShouldAllowAddSamePlayerToTheDepthChartDeeperDepth()
    {
        // Arrange
        AddTestPlayers();

        // Act
        _depthChartService.AddPlayerToDepthChart("NFL", "TB", "QB", 12, "Tom Brady", 3);

        // Assert
        AssertPlayerBackups("QB", "Tom Brady", []);
    }

    [Fact]
    public void TryRemoveNonExistingPlayers()
    {
        // Act
        var removedPlayer = _depthChartService.RemovePlayerFromDepthChart("NFL", "TB", "QB", "pudding");

        // Assert
        Assert.Equal([], removedPlayer);

        // Arrange
        AddTestPlayers();

        // Act
        removedPlayer = _depthChartService.RemovePlayerFromDepthChart("NFL", "TB", "QB", "pudding");

        // Assert
        Assert.Equal([], removedPlayer);
    }

    [Fact]
    public void TryToAddAndRemoveNonExistingPlayers()
    {
        // Arrange
        _depthChartService.AddPlayerToDepthChart("NFL", "TB", "LWR", 1, "pudding");

        // Act
        var removedPlayer = _depthChartService.RemovePlayerFromDepthChart("NFL", "TB", "LWR", "pudding");
        
        // Assert
        var expected = new[] { ("pudding", 1) }.ToList();
        Assert.Equal(expected[0].Item1, removedPlayer[0].PlayerName);
        Assert.Equal(expected[0].Item2, removedPlayer[0].PlayerNumber);

        // Act
        removedPlayer = _depthChartService.RemovePlayerFromDepthChart("NFL", "TB", "LWR", "pudding");

        // Assert
        Assert.Equal([], removedPlayer);
    }

    [Fact]
    public void VerifyDeptChartIsGood()
    {
        // Arrange
        AddTestPlayers();
        var depthChart = _depthChartService.GetFullDepthChart("NFL", "TB");
        
    }

    private void AssertPlayerBackups(string position, string playerName,
        (string playerName, int playerNumber)[] expectedBackups)
    {
        var backups = _depthChartService.GetBackups("NFL", "TB", position, playerName);
        Assert.Equal(expectedBackups.Length, backups.Count);

        for (var i = 0; i < expectedBackups.Length; i++)
        {
            Assert.Equal(expectedBackups[i].playerName, backups[i].PlayerName);
            Assert.Equal(expectedBackups[i].playerNumber, backups[i].PlayerNumber);
        }
    }

    public void Dispose()
    {
    }
}