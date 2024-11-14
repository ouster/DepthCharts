using System.ComponentModel.DataAnnotations;
using DepthCharts.Models;
using Microsoft.AspNetCore.Mvc;

namespace DepthCharts;

[ApiController]
[Route("api/[controller]")]
public class DepthChartController(
    DepthChartScraperService depthChartScraperService,
    DepthChartService depthChartService) : ControllerBase
{
    [HttpPost($"{{{nameof(sport)}}}/{{{nameof(team)}}}/player/{{{nameof(positionDepth)}}}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddPlayerToDepthChart(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport,
        [UppercaseOnly(ErrorMessage = "The team code must contain only uppercase letters.")]
        string team,
        [FromBody] PlayerDto player,
        int? positionDepth)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        depthChartService.AddPlayerToDepthChart(sport, team, player.Position, player.Number, player.Name,
            positionDepth);
        return Created();
    }

    [HttpDelete($"{{{nameof(sport)}}}/{{{nameof(team)}}}/{{{nameof(position)}}}/{{{nameof(player)}}}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public ActionResult<List<PlayerDto>> RemovePlayerFromDepthChart(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport,
        [UppercaseOnly(ErrorMessage = "The team code must contain only uppercase letters.")]
        string team, [UppercaseOnly(ErrorMessage = "The name must contain only uppercase letters.")] string position,
        [Required] string player)
    {
        ArgumentNullException.ThrowIfNull(player);
        depthChartService.RemovePlayerFromDepthChart(sport, team, position, player);
        return NoContent();
    }

    [HttpGet($"{{{nameof(sport)}}}/{{{nameof(team)}}}/backups/{{{nameof(position)}}}/{{{nameof(player)}}}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ActionResult<List<PlayerDto>> GetBackups(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport,
        [UppercaseOnly(ErrorMessage = "The team code must contain only uppercase letters.")]
        string team,
        [UppercaseOnly(ErrorMessage = "The player position must contain only uppercase letters.")]
        string position, [Required] string player)
    {
        var backups = depthChartService.GetBackups(sport, team, position, player);
        return Ok(backups);
    }

    [HttpGet($"{{{nameof(sport)}}}/{{{nameof(team)}}}/fullDepthChart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public OkObjectResult GetFullDepthChart(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport,
        [UppercaseOnly(ErrorMessage = "The team code must contain only uppercase letters.")]
        string team)
    {
        return Ok(depthChartService.GetFullDepthChart(sport, team));
    }


    // Helper endpoints to test scraping
    [HttpGet($"{{{nameof(sport)}}}/{{{nameof(team)}}}/scrape")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ScrapeTeamDepthChart(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport,
        [UppercaseOnly(ErrorMessage = "The team code must contain only uppercase letters.")]
        string team)
    {
        var depthChart = await depthChartScraperService.Scrape(sport, team);

        if (depthChart.TeamDepthChartGroupList != null)
            foreach (var player in depthChart.TeamDepthChartGroupList)
            {
                foreach (var entry in player.PositionsDepthList)
                {
                    depthChartService.AddPlayerToDepthChart(sport, team, player.Position, entry.PlayerNumber, entry.PlayerName);
                }
            }

        return Ok(depthChart);
    }

    [HttpGet($"{{{nameof(sport)}}}/scrape/teams")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ScrapeTeamDepthChartCodes(
        [UppercaseOnly(ErrorMessage = "The sport code must contain only uppercase letters.")]
        string sport)
    {
        var teamCodes = await depthChartScraperService.ScrapeTeamCodes(sport);

        return Ok(teamCodes);
    }
}