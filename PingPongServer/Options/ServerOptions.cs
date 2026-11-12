using System.ComponentModel.DataAnnotations;

namespace PingPongServer.Options;

/// <summary>Indeholder serverens port og maksimale antal samtidige klienter.</summary>
public sealed class ServerOptions
{
    public const string SectionName = "Server";

    [Range(1, 65535)]
    public int Port { get; set; }

    [Range(1, 10000)]
    public int MaxClients { get; set; }
}
