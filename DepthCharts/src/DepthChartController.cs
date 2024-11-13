using Microsoft.AspNetCore.Mvc;

namespace DepthCharts;

[ApiController]
[Route("api/[controller]")]
public class DepthChartController(DepthChartScraperService depthChartScraperService) : ControllerBase
{
    [HttpGet($"{{{nameof(sport)}}}/{{{nameof(team)}}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetTeamDepthChart(string sport, string team)
    {
        var depthChart = await depthChartScraperService.Scrape(sport, team);

        return Ok(depthChart);
    }
    
    [HttpGet($"{{{nameof(sport)}}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetTeamDepthChartCodes(string sport)
    {
        var teamCodes = await depthChartScraperService.ScrapeTeamCodes(sport);

        return Ok(teamCodes);
    }

    [HttpPost("player")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddPlayerToDepthChart([FromBody] PlayerDto player)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        throw new NotImplementedException();
        return Created();
    }

    [HttpDelete($"{{position}}/{{{nameof(player)}}}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public ActionResult<List<PlayerDto>> RemovePlayerFromDepthChart(int position, string player)
    {
        ArgumentNullException.ThrowIfNull(player);
        throw new NotImplementedException();
        return NoContent();
    }

    [HttpGet("backups/{{position}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ActionResult<List<PlayerDto>> GetBackups(string position)
    {
        throw new NotImplementedException();
        return Ok();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ParsedDepthChart GetFullDepthChart()
    {
        throw new NotImplementedException();
    }
}


public record ParsedDepthChart()
{
    public string? Team { get; init; }
    public List<PositionGroup>? ParsedDepthChartData { get; init; }
}

public record PositionGroup()
{
    public string? Section { get; init; }
    public string? Position { get; init; }
    public List<PlayerPosition>? PlayerPositions { get; init; }
}

public record PlayerPosition(string No, string PlayerName)
{
}

//TODO move to models 

public record PlayerDto()
{
    public string? Number { get; init; }
    public string? Name { get; init; }
    public string? Position { get; init; }
}

// public record DepthChart(string Team, List<PositionGroup> DepthChartData);
//
// public record PositionGroup(string Position, string Starter, List<string> Backups);
//
// public record PlayerDto(int Number, string Name, string Position);