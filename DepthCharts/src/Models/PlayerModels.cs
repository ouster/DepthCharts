namespace DepthCharts.Models;

public class PlayerEntryModel
{
    public int PlayerNumber { get; set; }
    public string PlayerName { get; set; }
    public long PositionDepth { get; set; }

    public PlayerEntryModel(int playerNumber, string playerName, long positionDepth)
    {
        PlayerNumber = playerNumber;
        PlayerName = playerName;
        PositionDepth = positionDepth;
    }
}