using System.ComponentModel.DataAnnotations;

namespace DepthCharts.Models;

public record PlayerDto
{
    public PlayerDto()
    {
        
    }
    public PlayerDto([Range(1, 99)] int Number, [Required] string Name, [Alphabetic(ErrorMessage = "The player position must contain only alphabetic characters.")] string Position)
    {
        this.Number = Number;
        this.Name = Name;
        this.Position = Position;
    }

    public int Number { get; init; }
    public string Name { get; init; }
    public string Position { get; init; }

    public void Deconstruct(out int Number, out string Name, out string Position)
    {
        Number = this.Number;
        Name = this.Name;
        Position = this.Position;
    }
}