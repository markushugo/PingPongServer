namespace PingPongServer.Domain;

public sealed class KnockKnockJoke
{
    public required string Who { get; init; }
    public required string Punchline { get; init; }
}
