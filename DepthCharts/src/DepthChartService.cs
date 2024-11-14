using System.Text.Json;
using AutoMapper;
using StackExchange.Redis;
using DepthCharts.Models;

namespace DepthCharts;

public class DepthChartService
{
    private readonly IMapper _mapper;
    private readonly IDatabase _redisDb;
    private readonly string _redisKeyPrefix;
    private readonly ConnectionMultiplexer _redis;

    public DepthChartService(IMapper mapper, string redisKeyPrefix)
    {
        ArgumentNullException.ThrowIfNull(redisKeyPrefix);
        _redisKeyPrefix = redisKeyPrefix;

        _mapper = mapper;
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION");

        var options = ConfigurationOptions.Parse(redisConnectionString ?? throw new InvalidOperationException());
        _redis = ConnectionMultiplexer.Connect(options);

        if (!_redis.IsConnected)
            throw new RedisException($"Unable to connect to the server {redisConnectionString}");

        _redisDb = _redis.GetDatabase();
    }

    public void AddPlayerToDepthChart(string sport, string team, string playerPosition, int playerNumber,
        string playerName, long? passedPositionDepth = null)
    {
        var sortedSetKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{playerPosition}";
        var hashKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{playerPosition}:players";

        var positionDepth = passedPositionDepth ?? _redisDb.SortedSetLength(sortedSetKey) + 1;

        // Shift players down if necessary
        var playersToShift = _redisDb.SortedSetRangeByScore(sortedSetKey, positionDepth);
        foreach (var p in playersToShift)
        {
            var currentDepth = _redisDb.SortedSetScore(sortedSetKey, p);
            if (currentDepth != null) _redisDb.SortedSetAdd(sortedSetKey, p, currentDepth.Value + 1);
        }

        var entry = new PlayerEntryModel(playerNumber, playerName);
        _redisDb.SortedSetAdd(sortedSetKey, JsonSerializer.Serialize(entry), positionDepth);

        // Add to hash for direct player lookup
        _redisDb.HashSet(hashKey, playerName, positionDepth);
    }

    public List<PlayerEntryModel> GetBackups(string sport, string team, string position, string playerName)
    {
        var sortedSetKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{position}";
        var hashKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{position}:players";

        var starterPlayerDepth = _redisDb.HashGet(hashKey, playerName);
        if (starterPlayerDepth.IsNull)
            return [];

        var backups = _redisDb.SortedSetRangeByScore(sortedSetKey, starterPlayerDepth+1, Double.PositiveInfinity);
        return backups
            .Where(p => !p.IsNullOrEmpty)
            .Select(p => JsonSerializer.Deserialize<PlayerEntryModel>(p!))
            .OfType<PlayerEntryModel>()
            .ToList();
    }

    public void RemovePlayerFromDepthChart(string sport, string team, string position, string playerName)
    {
        var sortedSetKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{position}";
        var hashKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{position}:players";

        var playerDepth = _redisDb.HashGet(hashKey, playerName);
        if (playerDepth.IsNull)
            return;

        _redisDb.SortedSetRemove(sortedSetKey, playerName);
        _redisDb.HashDelete(hashKey, playerName);

        // Shift players up after removal
        var playersToShift = _redisDb.SortedSetRangeByScore(sortedSetKey, (double)playerDepth);
        foreach (var p in playersToShift)
        {
            var currentDepth = _redisDb.SortedSetScore(sortedSetKey, p);
            if (currentDepth != null) _redisDb.SortedSetAdd(sortedSetKey, p, currentDepth.Value - 1);
        }
    }
    //
    // public ParsedDepthChartModel GetFullTeamDepthChart(string sport, string team)
    // {
    //     var key = $"{_redisKeyPrefix}-depthchart:{sport}:{team}";
    //     var chart = new ParsedDepthChartModel { Team = team, ParsedTeamDepthChartGroupList = new List<ParsedPositionGroupModel>() };
    //     var positions = _redisDb.HashGetAll(key);
    //
    //     foreach (var position in positions)
    //     {
    //         var posKey = position.Name.ToString();
    //         var players = _redisDb.SortedSetRangeByScoreWithScores(posKey);
    //
    //         var positionGroup = new ParsedPositionGroupModel
    //         {
    //             Position = posKey,
    //             PositionsDepthList =
    //                 players.Select(p => new PlayerDto { Name = p.Element, PlayerPosition = p.Score }).ToList()
    //         };
    //         chart.ParsedTeamDepthChartGroupList.Add(positionGroup);
    //     }
    //
    //     return chart;
    // }

    public PositionGroupModel GetPartialDepthChart(string sport, string team, string playerPosition)
    {
        var posKey = $"{_redisKeyPrefix}-depthchart:{sport}:{team}:{playerPosition}";
        var players = _redisDb.SortedSetRangeByScoreWithScores(posKey);
        var playerEntries = players.Select(p =>
            {
                var entry = JsonSerializer.Deserialize<PlayerEntryModel>(p.Element.ToString());
                return entry ?? throw new NullReferenceException();
            })
            .ToList();
        var positionGroup = new PositionGroupModel(playerPosition, playerEntries);
        
        return positionGroup;
    }
}