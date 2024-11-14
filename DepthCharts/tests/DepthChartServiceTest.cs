using AutoMapper;
using DepthCharts.Models;
using DotNet.Testcontainers.Containers;
using Testcontainers.Redis;
using Xunit;

namespace DepthCharts.tests;

public class DepthChartServiceTest : BaseServiceFixture, IDisposable
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .WithExposedPort(RedisPort)
        .WithReuse(true)
        .Build();
    private const int RedisPort = 6379;
    private DepthChartService? _depthChartService;
    
    private readonly List<(PlayerDto player, int depth)> _players =
    [
        (new PlayerDto(Name: "Tom Brady", Number: 12, Position: "QB"), 0),
        (new PlayerDto(Name: "Blaine Gabbert", Number: 11, Position: "QB"), 1),
        (new PlayerDto(Name: "Kyle Trask", Number: 2, Position: "QB"), 2),
        (new PlayerDto(Name: "Mike Evans", Number: 13, Position: "WR"), 0),
        (new PlayerDto(Name: "Jaelon Darden", Number: 1, Position: "WR"), 1),
        (new PlayerDto(Name: "Scott Miller", Number: 10, Position: "WR"), 2)
    ];


    private void InitializeSync()
    {
        // Start the Redis container
        _redisContainer.StartAsync().Wait();
        var mappedConnectionString = _redisContainer.GetConnectionString();
        // var mappedPort = _redisContainer.GetMappedPublicPort(RedisPort);
        Environment.SetEnvironmentVariable("REDIS_CONNECTION", mappedConnectionString+",SyncTimeout=60000");
        _depthChartService = new DepthChartService(Mapper, "unittests");
    }

    
    [Fact]
    public void AddPlayerToDepthChart_ShouldAddPlayersAtCorrectPositions()
    {
        // Arrange
        InitializeSync();

        // Act
        foreach (var (player, depth) in _players)
        {
            _depthChartService?.AddPlayerToDepthChart("NFL", "TB", player.Position, player.Number, player.Name, depth);
        }

        // Assert
        var qbDepthChart = _depthChartService?.GetBackups("NFL", "TB", "QB", "Tom Brady");
        Assert.Equal("Blaine Gabbert", qbDepthChart?[0].Name);
        Assert.Equal("Kyle Trask", qbDepthChart?[1].Name);

        var wrDepthChart = _depthChartService?.GetBackups("NFL", "TB", "WR", "Jaelon Darden");
        Assert.Equal("Scott Miller", wrDepthChart?[0].Name);
    }

    public async void Dispose()
    {
        if (_redisContainer.State == TestcontainersStates.Running)
        {
            await _redisContainer.StopAsync();
        }
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

