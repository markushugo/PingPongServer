using System.ComponentModel.DataAnnotations;

namespace PingPongServer.Options;

public sealed class JokeSelectionOptions
{
    public const string SectionName = "JokeSelection";

    [Required]
    [RegularExpression("^(Random|Sequential)$", ErrorMessage = "Strategy must be either 'Random' or 'Sequential'.")]
    public string Strategy { get; set; } = string.Empty;
}
