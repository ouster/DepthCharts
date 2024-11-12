using HtmlAgilityPack;
using System.Net.Http;
using Microsoft.Extensions.Options;


namespace DepthCharts;

public class NflDepthChartScraper(DepthChartScraperHttpClientHelper depthChartScraperHttpClientHelper, ILogger<NflDepthChartScraper> logger) : AbstractDepthChartScraper, IDepthChartScraper 
{
    public string Sport { get; } = "NFL";

    public async Task<ParsedDepthChart> GetTeamDepthChart(string teamName)
    {
        var html = await depthChartScraperHttpClientHelper.GetTeamDepthChartHtml(teamName);
        return GetTeamDepthChart(teamName, html);
    }

    public ParsedDepthChart GetTeamDepthChart(string teamName, string html)
    {
        var depthChart = new ParsedDepthChart
        {
            Team = teamName,
            ParsedDepthChartData = []
        };

        var document = AbstractDepthChartScraper.RemoveNbsp(html);

        // Refine table row selection for accuracy
        var table = document.DocumentNode.SelectSingleNode("//table[@id='gvChart']");
        if (table == null)
        {
            throw new DepthChartParseException("Table not found on the page.");
        }

        var rows = table.SelectNodes(".//tr");

        var section = "na";
        foreach (var row in rows)
        {
            var columns = row.SelectNodes("td")?.Where(cell => !string.IsNullOrWhiteSpace(cell.InnerText.Trim()))
                .ToList();

            if (columns == null)
                continue;
            if (columns.Count == 1)
                section = columns[0].InnerText.Trim();
            if (columns.Count < 3) continue; // Ensure we have at least three columns

            var position = columns[0].InnerText.Trim();

            var noPlayers = columns.Skip(1)
                .Select(col => col.InnerText.Trim())
                .Where(player => !string.IsNullOrEmpty(player))
                .ToList();

            if (noPlayers.Count % 2 != 0)
            {
                var msg = $"noPlayers List {noPlayers} for {teamName} is not an even number of pos/name pairs.";
                logger.LogError(msg);
                throw new DepthChartParseException(msg);
            }

            var players = new List<PlayerPosition>();
            for (var i = 0; i < noPlayers.Count - 1; i += 2)
            {
                var no = noPlayers[i];
                var name = noPlayers[i + 1];
                players.Add(new PlayerPosition(no, name));
            }

            depthChart.ParsedDepthChartData.Add(new PositionGroup
            {
                Section = section,
                Position = position,
                PlayerPositions = players
            });
        }

        if (depthChart.ParsedDepthChartData.Count != 0) return depthChart;

        logger.LogWarning($"Depth chart for {teamName} is empty");
        return depthChart;
    }


}

public class DepthChartParseException(string msg) : Exception(msg);