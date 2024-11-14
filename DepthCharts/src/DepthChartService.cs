using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts
{
    public class DepthChartService : IDepthChartService
    {
        public DepthChartService()
        {
            
        }
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, SortedList<long, PlayerEntryModel>>>>
            _depthCharts = new();

        public void AddPlayerToDepthChart(string sport, string team, string position, int playerNumber,
            string playerName,
            long? positionDepth = null)
        {
            if (!_depthCharts.ContainsKey(sport))
                _depthCharts[sport] = new Dictionary<string, Dictionary<string, SortedList<long, PlayerEntryModel>>>();

            if (!_depthCharts[sport].ContainsKey(team))
                _depthCharts[sport][team] = new Dictionary<string, SortedList<long, PlayerEntryModel>>();

            if (!_depthCharts[sport][team].ContainsKey(position))
                _depthCharts[sport][team][position] = new SortedList<long, PlayerEntryModel>();

            var depthChart = _depthCharts[sport][team][position];
            positionDepth ??= depthChart.Count + 1;

            // matching depth
            if (depthChart.ContainsKey(positionDepth.Value))
            {
                var playerEntry = depthChart[positionDepth.Value];

                // matching player name and number
                if (playerEntry.PlayerName == playerName && playerEntry.PlayerNumber == playerNumber)
                    return;

                foreach (var player in depthChart.Where(p => p.Key >= positionDepth.Value).OrderByDescending(p => p.Key)
                             .ToList())
                {
                    depthChart[player.Key + 1] = player.Value; // Shift player down
                    depthChart.Remove(player.Key);
                }
            }
            else // remove player if already present
            {
                RemovePlayerFromDepthChart(sport, team, position, playerName);
            }

            // Add the player at the specified depth
            var newPlayer = new PlayerEntryModel(playerNumber, playerName);
            depthChart.Add(positionDepth.Value, newPlayer);
        }

        public List<BackupPlayersDto> GetBackups(string sport, string team, string position, string playerName)
        {
            if (!_depthCharts.ContainsKey(sport) || !_depthCharts[sport].ContainsKey(team) ||
                !_depthCharts[sport][team].ContainsKey(position))
            {
                return [];
            }

            var depthChart = _depthCharts[sport][team][position];
            var playerKeyValuePair = depthChart.FirstOrDefault(p => p.Value.PlayerName == playerName);

            return depthChart
                .Where(p =>
                    p.Key > playerKeyValuePair.Key)
                .Select(p =>
                    new BackupPlayersDto(p.Value.PlayerNumber, p.Value.PlayerName)
                ).ToList();
        }

        public List<PlayerEntryModel> RemovePlayerFromDepthChart(string sport, string team, string position,
            string playerName)
        {
            if (!_depthCharts.ContainsKey(sport) || !_depthCharts[sport].ContainsKey(team) ||
                !_depthCharts[sport][team].ContainsKey(position))
            {
                return [];
            }

            var depthChart = _depthCharts[sport][team][position];

            if (depthChart.Values.Any(p => p.PlayerName == playerName)) // TODO playerNumber would be better to match on
            {
                var player = depthChart.FirstOrDefault(p => p.Value.PlayerName == playerName);

                depthChart.Remove(player.Key);

                // Shift players up if necessary
                var playersToShift = depthChart.Where(p => p.Key > player.Key).ToList();
                foreach (var (key, updatedPlayer) in playersToShift)
                {
                    var updatedKey = key - 1;
                    depthChart.Remove(key);
                    depthChart.Add(updatedKey, updatedPlayer);
                }

                return [player.Value];
            }

            return [];
        }

        public FullDepthChartDto GetFullDepthChart(string sport, string team)
        {
            if (!_depthCharts.ContainsKey(sport) || !_depthCharts[sport].ContainsKey(team))
            {
                return new FullDepthChartDto(sport, []);
            }

            var depthChart = _depthCharts[sport][team];
            var teamDepthChart = new FullDepthChartDto(
                sport,
                [
                    new TeamDepthChartDto(
                        team,
                        depthChart.Select(position => new PositionDepthChartDto(
                            position.Key,
                            position.Value.Values.ToList()
                        )).ToList()
                    )
                ]
            );

            return teamDepthChart;
        }

    }

    public record BackupPlayersDto(int PlayerNumber, string PlayerName);

    public record FullDepthChartDto(
        string Sport,
        List<TeamDepthChartDto> Teams
    );

    public record TeamDepthChartDto(
        string TeamName,
        List<PositionDepthChartDto> Positions
    );

    public record PositionDepthChartDto(
        string Position,
        List<PlayerEntryModel> Players
    );
    
    public interface IDepthChartService
    {
        void AddPlayerToDepthChart(string sport, string team, string position, int playerNumber,
            string playerName,
            long? positionDepth = null);

        List<BackupPlayersDto> GetBackups(string sport, string team, string position, string playerName);

        List<PlayerEntryModel>
            RemovePlayerFromDepthChart(string sport, string team, string position, string playerName);

        FullDepthChartDto GetFullDepthChart(
            string sport, string team);
    }
}