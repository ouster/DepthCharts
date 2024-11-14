using System.ComponentModel.DataAnnotations;

namespace DepthCharts;

public class AlphabeticAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var stringValue = value as string;
        return stringValue != null && stringValue.All(char.IsLetter);
    }
}