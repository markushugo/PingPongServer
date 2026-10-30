using System.Threading;
using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Options;

namespace PingPongServer.Selection;

public sealed class SequentialJokeSelector : IJokeSelector
{
    private readonly IReadOnlyList<KnockKnockJoke> _jokes;
    private long _nextIndex;

    public SequentialJokeSelector(IOptions<JokeCatalogOptions> options)
    {
        _jokes = options.Value.Items;
    }

    public KnockKnockJoke SelectJoke()
    {
        var index = Interlocked.Increment(ref _nextIndex) - 1;
        return _jokes[(int)((ulong)index % (ulong)_jokes.Count)];
    }
}
