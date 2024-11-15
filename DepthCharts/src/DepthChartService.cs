using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts
{
    /**
     * This is a example implementation keeping the charts in memory
     * Even if you scrape all of ourlads it's not a lot of data to keep in memory
     * For production, would probably look at a DB or Redis + Redis.OM
     * as you would want to add more sports
     */
    public class DepthChartService : IDepthChartService
    {
        public DepthChartService()
        {
        }

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string,
                ConcurrentDictionary<string, SortedDictionary<long, PlayerEntryModel>>>>
            _depthCharts = new();

        private readonly ReaderWriterLockSlim _lock = new();

        public void AddPlayerToDepthChart(string sport, string team, string position, int playerNumber,
            string playerName,
            long? positionDepth = null)
        {
            var depthChart = GetOrAddDepthChart();

            positionDepth ??= depthChart.Count + 1;


            // Check if the player is already at the specified depth with matching details
            if (depthChart.TryGetValue(positionDepth.Value, out var existingPlayer) &&
                existingPlayer.PlayerNumber == playerNumber && existingPlayer.PlayerName == playerName)
            {
                // Player already exists at this depth, so skip adding
                return;
            }

            RemovePlayerFromDepthChart(sport, team, position, playerName);

            _lock.EnterWriteLock();
            try
            {
                // Shift players down to make room for the new entry
                for (long i = depthChart.Count; i >= positionDepth.Value; i--)
                {
                    if (!depthChart.TryGetValue(i, out var playerToShift)) continue;
                    depthChart[i + 1] = playerToShift;
                    depthChart.Remove(i);
                }

                // Add the new player at the specified depth
                depthChart[positionDepth.Value] = new PlayerEntryModel(playerNumber, playerName);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return;

            SortedDictionary<long, PlayerEntryModel> GetOrAddDepthChart()
            {
                var depthChart = _depthCharts.GetOrAdd(sport,
                        _ => new ConcurrentDictionary<string,
                            ConcurrentDictionary<string, SortedDictionary<long, PlayerEntryModel>>>())
                    .GetOrAdd(team, _ => new ConcurrentDictionary<string, SortedDictionary<long, PlayerEntryModel>>())
                    .GetOrAdd(position, _ => new SortedDictionary<long, PlayerEntryModel>());
                return depthChart;
            }
        }

        public List<BackupPlayersDto> GetBackups(string sport, string team, string position, string playerName)
        {
            if (!TryGetDepthChart(sport, team, position, out var depthChart))
            {
                return [];
            }


            var playerEntry = depthChart.FirstOrDefault(p => p.Value.PlayerName == playerName);
            if (playerEntry.Value == null)
            {
                return [];
            }

            _lock.EnterReadLock();
            try
            {
                return depthChart
                    .Where(p => p.Key > playerEntry.Key)
                    .Select(p => new BackupPlayersDto(p.Value.PlayerNumber, p.Value.PlayerName))
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private bool TryGetDepthChart(string sport, string team, string position,
            out SortedDictionary<long, PlayerEntryModel> depthChart)
        {
            depthChart = [];
            return _depthCharts.TryGetValue(sport, out var sportTeams) &&
                   sportTeams.TryGetValue(team, out var teamPositions) &&
                   teamPositions.TryGetValue(position, out depthChart);
        }

        public List<PlayerEntryModel> RemovePlayerFromDepthChart(string sport, string team, string position,
            string playerName)
        {
            if (!TryGetDepthChart(sport, team, position, out var depthChart))
            {
                return [];
            }

            _lock.EnterWriteLock();
            try
            {
                if (depthChart.Values.Any(p =>
                        p.PlayerName == playerName)) // TODO playerNumber would be better to match on?
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
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return [];
        }

        public FullDepthChartDto GetFullDepthChart(string sport, string team)
        {
            _lock.EnterReadLock();
            try
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
            finally
            {
                _lock.ExitReadLock();
            }
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