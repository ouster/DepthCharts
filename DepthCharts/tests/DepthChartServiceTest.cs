using AutoMapper;
using DepthCharts.Models;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace DepthCharts.tests;

public class DepthChartServiceTest(DepthChartService depthChartService) : BaseServiceFixture, IDisposable
{
    private readonly List<(PlayerDto player, int depth)> _players =
    [
        (new PlayerDto(Name: "Tom Brady", Number: 12, Position: "QB"), 0),
        (new PlayerDto(Name: "Blaine Gabbert", Number: 11, Position: "QB"), 1),
        (new PlayerDto(Name: "Kyle Trask", Number: 2, Position: "QB"), 2),
        (new PlayerDto(Name: "Mike Evans", Number: 13, Position: "WR"), 0),
        (new PlayerDto(Name: "Jaelon Darden", Number: 1, Position: "WR"), 1),
        (new PlayerDto(Name: "Scott Miller", Number: 10, Position: "WR"), 2)
    ];

    [Fact]
    public void AddPlayerToDepthChart_ShouldAddPlayersAtCorrectPositions()
    {
        // Arrange

        // Act
        foreach (var (player, depth) in _players)
        {
            depthChartService?.AddPlayerToDepthChart("NFL", "TB", player.Position, player.Number, player.Name, depth);
        }

        // Assert
        var qbDepthChart = depthChartService?.GetBackups("NFL", "TB", "QB", "Tom Brady");
        Assert.Equal("Blaine Gabbert", qbDepthChart?[0].PlayerName);
        Assert.Equal("Kyle Trask", qbDepthChart?[1].PlayerName);

        var wrDepthChart = depthChartService?.GetBackups("NFL", "TB", "WR", "Jaelon Darden");
        Assert.Equal("Scott Miller", wrDepthChart?[0].PlayerName);
    }

    public void Dispose()
    {
    }
}

public abstract class AutoMapperFixture : IDisposable
{
    public static IMapper MapperFactory()
    {
        // Set up a service provider with AutoMapper for testing
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAutoMapper(typeof(Program)); // Register AutoMapper with all profiles  

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        return mapper;
    }

    public void Dispose()
    {
    }
}

public class BaseServiceFixture
{
    protected readonly IMapper Mapper;

    protected BaseServiceFixture()
    {
        Mapper = AutoMapperFixture.MapperFactory();
    }
}

