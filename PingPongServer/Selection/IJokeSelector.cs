using PingPongServer.Domain;

namespace PingPongServer.Selection;

/// <summary>Definerer hvordan den næste joke vælges.</summary>
public interface IJokeSelector
{
    /// <summary>Returnerer den næste joke til en samtale.</summary>
    KnockKnockJoke SelectJoke();
}
