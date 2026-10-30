using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Options;

namespace PingPongServer.Selection;

public sealed class RandomJokeSelector : IJokeSelector
{
    private readonly IReadOnlyList<KnockKnockJoke> _jokes;
    private readonly AsyncLocal<KnockKnockJoke?> _lastJoke = new();

    public RandomJokeSelector(IOptions<JokeCatalogOptions> options)
    {
        _jokes = options.Value.Items;
    }

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

    private static bool IsSameJoke(
        KnockKnockJoke first,
        KnockKnockJoke? second) =>
        second is not null &&
        first.Who == second.Who &&
        first.Punchline == second.Punchline;
}
