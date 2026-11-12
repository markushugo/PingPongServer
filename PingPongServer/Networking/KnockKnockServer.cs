using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Options;

namespace PingPongServer.Networking;

/// <summary>Modtager TCP-klienter og starter en separat jokesamtale for hver klient.</summary>
public sealed class KnockKnockServer(
    IOptions<ServerOptions> options,
    KnockKnockConversation conversation,
    ILogger<KnockKnockServer> logger) : BackgroundService
{
    private readonly SemaphoreSlim _clientSlots = new(options.Value.MaxClients, options.Value.MaxClients);
    private readonly ConcurrentDictionary<Task, byte> _clientTasks = new();
    private readonly int _port = options.Value.Port;
    private TcpListener? _listener;

    /// <summary>Lytter efter klienter, indtil applikationen bliver stoppet.</summary>
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
                var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "ukendt";
                // Klienten afvises straks, hvis alle tilladte pladser er optaget.
                if (!_clientSlots.Wait(0))
                {
                    logger.LogWarning("Klient {Endpoint} afvist: maksimum antal klienter.", endpoint);
                    try
                    {
                        await RejectClientAsync(client, stoppingToken);
                    }
                    catch (IOException)
                    {
                        // Klienten kan lukke, før afvisningen er sendt.
                    }
                    finally
                    {
                        logger.LogInformation("Klient {Endpoint} disconnected (afvist).", endpoint);
                    }
                    continue;
                }

                // Opgaven gemmes, så serveren kan vente på aktive klienter ved nedlukning.
                logger.LogInformation("Klient {Endpoint} connected.", endpoint);
                var task = HandleClientAsync(client, endpoint, stoppingToken);
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

    /// <summary>Stopper listeneren og venter på igangværende klientsamtaler.</summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        await base.StopAsync(cancellationToken);
        await Task.WhenAll(_clientTasks.Keys);
        _clientSlots.Dispose();
    }

    /// <summary>Opretter en session og gennemfører samtalen med én klient.</summary>
    private async Task HandleClientAsync(TcpClient client, string endpoint, CancellationToken cancellationToken)
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
            logger.LogInformation("Klient {Endpoint} disconnected.", endpoint);
        }
    }

    /// <summary>Sender en forklaring og lukker forbindelsen til en overskydende klient.</summary>
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
