using System.ComponentModel.DataAnnotations;
using PingPongServer.Domain;

namespace PingPongServer.Options;

/// <summary>Indlæser og validerer listen over konfigurerede jokes.</summary>
public sealed class JokeCatalogOptions : IValidatableObject
{
    public const string SectionName = "Jokes";

    [MinLength(1, ErrorMessage = "At least one joke must be configured.")]
    public List<KnockKnockJoke> Items { get; set; } = [];

    /// <summary>Kontrollerer at hver joke både har et navn og en pointe.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        for (var index = 0; index < Items.Count; index++)
        {
            var joke = Items[index];
            if (string.IsNullOrWhiteSpace(joke.Who))
            {
                yield return new ValidationResult($"Jokes:Items:{index}:Who is required.");
            }

            if (string.IsNullOrWhiteSpace(joke.Punchline))
            {
                yield return new ValidationResult($"Jokes:Items:{index}:Punchline is required.");
            }
        }
    }
}
