namespace DepthCharts.Models;

public class PlayerEntryModel
{
    public int PlayerNumber { get; set; }
    public string PlayerName { get; set; }

    public PlayerEntryModel(int playerNumber, string playerName)
    {
        PlayerNumber = playerNumber;
        PlayerName = playerName;
    }
}