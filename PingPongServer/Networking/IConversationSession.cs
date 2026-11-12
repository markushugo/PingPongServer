namespace PingPongServer.Networking;

/// <summary>Abstraherer læsning og skrivning i en klientsamtale.</summary>
public interface IConversationSession
{
    /// <summary>Læs en tekstlinje fra klienten.</summary>
    Task<string?> ReadLineAsync(CancellationToken cancellationToken);

    /// <summary>Send en tekstlinje til klienten.</summary>
    Task WriteLineAsync(string message, CancellationToken cancellationToken);
}
