using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Options;

namespace PingPongServer.Networking;

public sealed class KnockKnockServer(
    IOptions<ServerOptions> options,
    KnockKnockConversation conversation,
    ILogger<KnockKnockServer> logger) : BackgroundService
{
    private readonly SemaphoreSlim _clientSlots = new(options.Value.MaxClients, options.Value.MaxClients);
    private readonly ConcurrentDictionary<Task, byte> _clientTasks = new();
    private readonly int _port = options.Value.Port;
    private TcpListener? _listener;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        logger.LogInformation("Knock Knock Server listening on port {Port}.", _port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                if (!_clientSlots.Wait(0))
                {
                    await RejectClientAsync(client, stoppingToken);
                    continue;
                }

                var task = HandleClientAsync(client, stoppingToken);
                _clientTasks.TryAdd(task, 0);
                _ = task.ContinueWith(
                    completedTask => _clientTasks.TryRemove(completedTask, out _),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Server is shutting down.");
        }
        catch (SocketException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Listener stopped during shutdown.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        await base.StopAsync(cancellationToken);
        await Task.WhenAll(_clientTasks.Keys);
        _clientSlots.Dispose();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using var disposableClient = client;
            using var stream = client.GetStream();
            using var session = new NetworkConversationSession(stream);
            await conversation.RunAsync(session, cancellationToken);
        }
        catch (IOException)
        {
            logger.LogInformation("A client disconnected unexpectedly.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _clientSlots.Release();
        }
    }

    private static async Task RejectClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            await writer.WriteLineAsync(
                "Serveren har nået maksimum antal klienter. Prøv igen senere.".AsMemory(),
                cancellationToken);
        }
    }
}
