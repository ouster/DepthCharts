using HtmlAgilityPack;
using System.Net.Http;
using Microsoft.Extensions.Options;


namespace DepthCharts;

public class NflDepthChartScraper(NflDepthChartScraperHttpClientHelper nflDepthChartScraperHttpClientHelper, ILogger<NflDepthChartScraper> logger) : AbstractDepthChartScraper, IDepthChartScraper 
{
    public string Sport { get; } = "NFL";

    public async Task<ParsedDepthChart> GetTeamDepthChart(string teamName)
    {
        var html = await nflDepthChartScraperHttpClientHelper.GetTeamDepthChartHtml(teamName);
        return GetTeamDepthChart(teamName, html);
    }

    public ParsedDepthChart GetTeamDepthChart(string teamName, string html)
    {
        var depthChart = new ParsedDepthChart
        {
            Team = teamName,
            ParsedDepthChartData = []
        };

        var document = RemoveNbsp(html);

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
            if (columns.Count < 3) continue; 

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

    public async Task<SortedSet<string>> GetTeamDepthChartCodes()
    {
        var html = await nflDepthChartScraperHttpClientHelper.GetTeamDepthChartCodesHtml();
        return GetTeamDepthChartCodes(html);
    }
    
    public SortedSet<string> GetTeamDepthChartCodes(string html){
        var document = RemoveNbsp(html);
        
        var table = document.DocumentNode.SelectSingleNode("//table[@id='gvChart']");
        if (table == null)
        {
            throw new DepthChartParseException("Table not found on the page.");
        }

        SortedSet<string> codes = [];
        
        var rows = table.SelectNodes(".//tr");
        foreach (var row in rows)
        {
            var columns = row.SelectNodes("td")?.Where(cell => !string.IsNullOrWhiteSpace(cell.InnerText.Trim()))
                .ToList();

            if (columns == null)
                continue;
            if (columns.Count >= 3)
            {
                var teamCode = columns[0].InnerText.Trim();
                codes.Add(teamCode);
            }
        }

        if (codes.Count != Nofltteams)
        {
            logger.LogWarning($"Expected {Nofltteams} team codes, got {codes.Count}");
        }

        return codes;
    }

    public const int Nofltteams = 32;
}

public class DepthChartParseException(string msg) : Exception(msg);