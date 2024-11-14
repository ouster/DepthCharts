using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts
{
    public class DepthChartService : IDepthChartService
    {
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

        public List<PlayerEntryModel> GetBackups(string sport, string team, string position, string playerName)
        {
            if (!_depthCharts.ContainsKey(sport) || !_depthCharts[sport].ContainsKey(team) ||
                !_depthCharts[sport][team].ContainsKey(position))
            {
                return [];
            }

            var depthChart = _depthCharts[sport][team][position];
            var playerKeyValuePair = depthChart.FirstOrDefault(p => p.Value.PlayerName == playerName);

            return depthChart.Where(p => p.Key > playerKeyValuePair.Key).Select(p => p.Value).ToList();
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

        public void GetFullDepthChart(string sport, string team)
        {
            throw new NotImplementedException();
        }
    }

    public interface IDepthChartService
    {
        void AddPlayerToDepthChart(string sport, string team, string position, int playerNumber,
            string playerName,
            long? positionDepth = null);

        List<PlayerEntryModel> GetBackups(string sport, string team, string position, string playerName);

        List<PlayerEntryModel>
            RemovePlayerFromDepthChart(string sport, string team, string position, string playerName);

        void GetFullDepthChart(string sport, string team);
    }
}