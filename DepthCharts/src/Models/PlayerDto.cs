using System.ComponentModel.DataAnnotations;

namespace DepthCharts.Models;

public record PlayerDto([Range(1, 99)] int Number, [Required] string Name, [Alphabetic(ErrorMessage = "The player position must contain only alphabetic characters.")] string Position);