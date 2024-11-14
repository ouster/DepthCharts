using AutoMapper;
using DepthCharts.Models;
using Moq;
using Xunit;

namespace DepthCharts.tests;

public class DepthChartServiceTests : BaseServiceFixture, IDisposable
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

    private readonly DepthChartService _depthChartService;
    
    public DepthChartServiceTests()
    {
        _depthChartService = new DepthChartService();
    }

    [Fact]
    public void AddPlayerToDepthChart_ShouldAddPlayersAtCorrectPositions()
    {
        // Arrange
    
        // Act
        foreach (var (player, depth) in _players)
        {
            _depthChartService?.AddPlayerToDepthChart("NFL", "TB", player.Position, player.Number, player.Name, depth);
        }
    
        // Assert
        var qbDepthChart = _depthChartService?.GetBackups("NFL", "TB", "QB", "Tom Brady");
        Assert.Equal("Blaine Gabbert", qbDepthChart?[0].PlayerName);
        Assert.Equal(11, qbDepthChart?[0].PlayerNumber);
        Assert.Equal("Kyle Trask", qbDepthChart?[1].PlayerName);
        Assert.Equal(2, qbDepthChart?[1].PlayerNumber);
    
        var wrDepthChart = _depthChartService?.GetBackups("NFL", "TB", "WR", "Jaelon Darden");
        Assert.Equal("Scott Miller", wrDepthChart?[0].PlayerName);
        Assert.Equal(10, wrDepthChart?[0].PlayerNumber);
        
        qbDepthChart = _depthChartService?.GetBackups("NFL", "TB", "QB", "Mike Evans");
        Assert.Empty(qbDepthChart);
        
        qbDepthChart = _depthChartService?.GetBackups("NFL", "TB", "QB", "Kyle Trask");
        Assert.Empty(qbDepthChart);
        
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

