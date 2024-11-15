using HtmlAgilityPack;
using System.Net.Http;
using DepthCharts.Models;
using Microsoft.Extensions.Options;


namespace DepthCharts;

public class NflDepthChartScraper(INflDepthChartScraperHttpClientHelper nflDepthChartScraperHttpClientHelper, ILogger<NflDepthChartScraper> logger) : AbstractDepthChartScraper, IDepthChartScraper 
{
    public string Sport { get; } = "NFL";

    public async Task<DepthChartModel> GetTeamDepthChart(string teamName)
    {
        var html = await nflDepthChartScraperHttpClientHelper.GetTeamDepthChartHtml(teamName);
        return GetTeamDepthChart(teamName, html);
    }

    public DepthChartModel GetTeamDepthChart(string teamName, string html)
    {
        var depthChart = new DepthChartModel
        {
            Team = teamName,
            TeamDepthChartGroupList = []
        };

        var document = RemoveNbsp(html);

        var table = document.DocumentNode.SelectSingleNode("//table[@id='gvChart']");
        if (table == null)
        {
            throw new DepthChartParseException("Table not found on the page.");
        }

        var rows = table.SelectNodes(".//tr");

        foreach (var row in rows)
        {
            var columns = row.SelectNodes("td")?.Where(cell => !string.IsNullOrWhiteSpace(cell.InnerText.Trim()))
                .ToList();

            if (columns == null)
                continue;

            if (columns.Count < 3) continue; 

            var position = columns[0].InnerText.Trim();

            var noPlayers = columns.Skip(1)
                .Select(col => col.InnerText.Trim())
                .ToList();

            if (noPlayers.Count % 2 != 0)
            {
                noPlayers = RepairData(noPlayers);
                var msg = $"noPlayers List {noPlayers} for {teamName} was adjusted to an even number of pos/name pairs.";
                logger.LogWarning(msg);
            }

            var players = new List<PlayerEntryModel>();
            for (var i = 0; i < noPlayers.Count - 1; i += 2)
            {
                var no = 0;
                if (string.IsNullOrEmpty(noPlayers[i]))
                {
                    logger.LogWarning($"blank cell in player list {noPlayers[i]} for {teamName}");
                }
                else
                {
                    no = Convert.ToInt32(noPlayers[i].Trim());
                }

                var name = noPlayers[i + 1].Trim();
                players.Add(new PlayerEntryModel(no, name));
            }

            depthChart.TeamDepthChartGroupList.Add(new PositionGroupModel(Position: position, players));
        }

        if (depthChart.TeamDepthChartGroupList.Count != 0) return depthChart;

        logger.LogWarning($"Depth chart for {teamName} is empty");
        return depthChart;
    }

    // internal static List<string> RepairData(List<string> originalList)
    // {
    //     var newList = new List<string>();
    //
    //     foreach (var entry in originalList)
    //     {
    //         if (entry is int || entry is float)  // Check if entry is a number
    //         {
    //             newList.Add(entry);
    //         }
    //         else if (entry is string)  // Check if entry is a name
    //         {
    //             newList.Add("0");  // Add 0 before the name
    //             newList.Add(entry);
    //         }
    //     }
    //
    //     return newList;
    // }

    internal static List<string> RepairData(List<string> originalList)
    {
        var newList = new List<string>();
        bool expectingName = false; // Track if the next entry should be a name

        foreach (var entry in originalList)
        {
            if (int.TryParse(entry, out _)) // Check if the entry can be parsed as an integer
            {
                if (expectingName)
                {
                    // If we were expecting a name but found a number, add a placeholder name
                    newList.Add("Unnamed"); // Placeholder name
                }
                newList.Add(entry); // Add the number
                expectingName = true; // Next, we expect a name
            }
            else // Treat as a name
            {
                if (!expectingName)
                {
                    // If we encounter a name without a preceding number, add a 0
                    newList.Add("0");
                }
                newList.Add(entry); // Add the name
                expectingName = false; // Reset the expectation since we just added a name
            }
        }

        // If we end with a number, add a placeholder name
        if (expectingName)
        {
            newList.Add("Unnamed"); // Placeholder for the last number
        }

        return newList;
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