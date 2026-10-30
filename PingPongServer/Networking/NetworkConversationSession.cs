using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PingPongServer.Networking;

public sealed class NetworkConversationSession : IConversationSession, IDisposable
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public NetworkConversationSession(NetworkStream stream)
    {
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        _writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true)
        {
            AutoFlush = true
        };
    }

    public Task<string?> ReadLineAsync(CancellationToken cancellationToken) =>
        _reader.ReadLineAsync(cancellationToken).AsTask();

    public Task WriteLineAsync(string message, CancellationToken cancellationToken) =>
        _writer.WriteLineAsync(message.AsMemory(), cancellationToken);

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
    }
}
