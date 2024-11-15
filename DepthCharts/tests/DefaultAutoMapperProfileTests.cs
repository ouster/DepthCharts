using DepthCharts.Models;

namespace DepthCharts.tests;

using AutoMapper;
using DepthCharts;
using Xunit;

public class DefaultAutomapperProfileTests
{
    private readonly IMapper _mapper;

    public DefaultAutomapperProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DefaultAutomapperProfile>();
        });

        _mapper = config.CreateMapper();
        config.AssertConfigurationIsValid(); // Ensures mappings are configured correctly
    }

    [Fact]
    public void Should_Map_PlayerEntryModel_To_PlayerDto()
    {
        // Arrange
        var playerEntry = new PlayerEntryModel(10, "John Doe");

        // Act
        var playerDto = _mapper.Map<PlayerDto>(playerEntry);

        // Assert
        Assert.Equal(playerEntry.PlayerNumber, playerDto.Number);
        Assert.Equal(playerEntry.PlayerName, playerDto.Name);
    }

    [Fact]
    public void Should_Map_PlayerDto_To_PlayerEntryModel()
    {
        // Arrange
        var playerDto = new PlayerDto(15, "Jane Doe", "TeamA");

        // Act
        var playerEntry = _mapper.Map<PlayerEntryModel>(playerDto);

        // Assert
        Assert.Equal(playerDto.Number, playerEntry.PlayerNumber);
        Assert.Equal(playerDto.Name, playerEntry.PlayerName);
    }
}
