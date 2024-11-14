using DepthCharts.Models;
using Microsoft.AspNetCore.Mvc;

namespace DepthCharts;

[ApiController]
[Route("api/[controller]")]
public class DepthChartController(DepthChartScraperService depthChartScraperService, DepthChartService depthChartService) : ControllerBase
{
    [HttpPost($"{{{nameof(sport)}}}/{{{nameof(team)}}}/player" + "/{positionDepth}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddPlayerToDepthChart(string sport, string team, [FromBody] PlayerDto player, int? positionDepth)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        depthChartService.AddPlayerToDepthChart(sport, team, player.Position, player.Number, player.Name, positionDepth);
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
    public DepthChartModel GetFullDepthChart()
    {
        throw new NotImplementedException();
    }
    
    
    // Helper endpoints to test scraping
    [HttpGet($"{{{nameof(sport)}}}/{{{nameof(team)}}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ScrapeTeamDepthChart(string sport, string team)
    {
        var depthChart = await depthChartScraperService.Scrape(sport, team);

        return Ok(depthChart);
    }
    
    [HttpGet($"{{{nameof(sport)}}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ScrapeTeamDepthChartCodes(string sport)
    {
        var teamCodes = await depthChartScraperService.ScrapeTeamCodes(sport);

        return Ok(teamCodes);
    }

}
