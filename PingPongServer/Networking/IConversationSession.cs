namespace PingPongServer.Networking;

public interface IConversationSession
{
    Task<string?> ReadLineAsync(CancellationToken cancellationToken);
    Task WriteLineAsync(string message, CancellationToken cancellationToken);
}
