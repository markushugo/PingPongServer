using PingPongServer.Domain;

namespace PingPongServer.Selection;

public interface IJokeSelector
{
    KnockKnockJoke SelectJoke();
}
