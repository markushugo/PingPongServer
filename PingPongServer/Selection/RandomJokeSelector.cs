using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Options;

namespace PingPongServer.Selection;

/// <summary>Vælger en tilfældig joke uden straks at gentage den seneste.</summary>
public sealed class RandomJokeSelector : IJokeSelector
{
    private readonly IReadOnlyList<KnockKnockJoke> _jokes;
    private readonly AsyncLocal<KnockKnockJoke?> _lastJoke = new();

    /// <summary>Henter de validerede jokes fra konfigurationen.</summary>
    public RandomJokeSelector(IOptions<JokeCatalogOptions> options)
    {
        _jokes = options.Value.Items;
    }

    /// <inheritdoc />
    public KnockKnockJoke SelectJoke()
    {
        var previousJoke = _lastJoke.Value;
        KnockKnockJoke selectedJoke;

        do
        {
            selectedJoke = _jokes[Random.Shared.Next(_jokes.Count)];
        }
        while (_jokes.Count > 1 && IsSameJoke(selectedJoke, previousJoke));

        _lastJoke.Value = selectedJoke;
        return selectedJoke;
    }

    /// <summary>Sammenligner både navn og pointe for to jokes.</summary>
    private static bool IsSameJoke(
        KnockKnockJoke first,
        KnockKnockJoke? second) =>
        second is not null &&
        first.Who == second.Who &&
        first.Punchline == second.Punchline;
}
