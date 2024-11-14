namespace DepthCharts.Models;

public record DepthChartModel()
{
    public string? Team { get; init; }
    public List<PositionGroupModel>? TeamDepthChartGroupList { get; init; }
}

public record PositionGroupModel(string Position, List<PlayerEntryModel> PositionsDepthList);

// public record PlayerPositionDepth(string Position, string Name);