namespace PingPongServer.Domain;

/// <summary>Indeholder navnet og pointen til én Knock Knock-joke.</summary>
public sealed class KnockKnockJoke
{
    public required string Who { get; init; }
    public required string Punchline { get; init; }
}
