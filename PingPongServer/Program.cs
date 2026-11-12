using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PingPongServer.Domain;
using PingPongServer.Networking;
using PingPongServer.Options;
using PingPongServer.Selection;

namespace PingPongServer;

/// <summary>Konfigurerer og starter PingPong-serveren.</summary>
internal static class Program
{
    /// <summary>Indlæser konfiguration, registrerer services og kører serveren.</summary>
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services
            .AddOptions<ServerOptions>()
            .BindConfiguration(ServerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddOptions<JokeSelectionOptions>()
            .BindConfiguration(JokeSelectionOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddOptions<JokeCatalogOptions>()
            .BindConfiguration(JokeCatalogOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Begge strategier registreres, så konfigurationen kan vælge den aktive.
        builder.Services.AddSingleton<RandomJokeSelector>();
        builder.Services.AddSingleton<SequentialJokeSelector>();
        builder.Services.AddSingleton<IJokeSelector>(serviceProvider =>
        {
            var selectionOptions = serviceProvider
                .GetRequiredService<IOptions<JokeSelectionOptions>>()
                .Value;

            return selectionOptions.Strategy.Equals("Sequential", StringComparison.OrdinalIgnoreCase)
                ? serviceProvider.GetRequiredService<SequentialJokeSelector>()
                : serviceProvider.GetRequiredService<RandomJokeSelector>();
        });

        builder.Services.AddTransient<KnockKnockConversation>();
        builder.Services.AddHostedService<KnockKnockServer>();

        // Konfigurationsfejl vises samlet og afslutter programmet med en fejlkode.
        try
        {
            await builder.Build().RunAsync();
        }
        catch (OptionsValidationException exception)
        {
            Console.Error.WriteLine("Konfigurationsfejl:");
            foreach (var failure in exception.Failures)
            {
                Console.Error.WriteLine($"- {failure}");
            }

            Environment.ExitCode = 1;
        }
    }
}
