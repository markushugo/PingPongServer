using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PingPongServer.Networking;

/// <summary>Tilpasser en TCP-stream til serverens linjebaserede samtaleinterface.</summary>
public sealed class NetworkConversationSession : IConversationSession, IDisposable
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    /// <summary>Opretter UTF-8-læsere og -skrivere omkring klientens netværksstream.</summary>
    public NetworkConversationSession(NetworkStream stream)
    {
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        _writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true)
        {
            //intet delay, sender med det samme
            AutoFlush = true
        };
    }

    /// <inheritdoc />
    public Task<string?> ReadLineAsync(CancellationToken cancellationToken) =>
        _reader.ReadLineAsync(cancellationToken).AsTask();

    /// <inheritdoc />
    public Task WriteLineAsync(string message, CancellationToken cancellationToken) =>
        _writer.WriteLineAsync(message.AsMemory(), cancellationToken);

    /// <summary>Frigiver sessionens reader og writer.</summary>
    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
    }
}
