using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts
{
    public class DepthChartService
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

            // Shift players down if necessary
            if (depthChart.ContainsKey(positionDepth.Value))
            {
                foreach (var player in depthChart.Where(p => p.Key >= positionDepth.Value).OrderByDescending(p => p.Key)
                             .ToList())
                {
                    depthChart[player.Key + 1] = player.Value; // Shift player down
                    depthChart.Remove(player.Key);
                }
            }

            // Add the player at the specified depth
            var newPlayer = new PlayerEntryModel(playerNumber, playerName, positionDepth.Value);
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
            var player = depthChart.Values.FirstOrDefault(p => p.PlayerName == playerName);

            if (player == null)
            {
                return [];
            }

            return depthChart.Values.Where(p => p.PositionDepth > player.PositionDepth).ToList();
        }

        public void RemovePlayerFromDepthChart(string sport, string team, string position, string playerName)
        {
            if (!_depthCharts.ContainsKey(sport) || !_depthCharts[sport].ContainsKey(team) ||
                !_depthCharts[sport][team].ContainsKey(position))
            {
                return;
            }

            var depthChart = _depthCharts[sport][team][position];
            var player = depthChart.Values.FirstOrDefault(p => p.PlayerName == playerName);

            if (player != null)
            {
                depthChart.Remove(player.PositionDepth);

                // Shift players up if necessary
                var playersToShift = depthChart.Where(p => p.Key > player.PositionDepth).ToList();
                foreach (var playerToShift in playersToShift)
                {
                    var updatedPlayer = playerToShift.Value;
                    updatedPlayer.PositionDepth--;
                    depthChart.Remove(playerToShift.Key);
                    depthChart.Add(updatedPlayer.PositionDepth, updatedPlayer);
                }
            }
        }
    }
}