using PingPongServer.Networking;
using PingPongServer.Selection;

namespace PingPongServer.Domain;

/// <summary>Styrer forløbet i en Knock Knock-samtale med én klient.</summary>
public sealed class KnockKnockConversation(IJokeSelector jokeSelector)
{
    /// <summary>Gennemfører jokes, indtil klienten afslutter eller afbryder forbindelsen.</summary>
    public async Task RunAsync(IConversationSession session, CancellationToken cancellationToken)
    {
        var joke = jokeSelector.SelectJoke();
        var state = ConversationState.WaitingForWho;

        await session.WriteLineAsync("Knock! Knock!", cancellationToken);

        while (state != ConversationState.Finished)
        {
            var input = await session.ReadLineAsync(cancellationToken);
            if (input is null)
            {
                return;
            }

            var response = input.Trim();
            // Den aktuelle tilstand afgør, hvilket input der accepteres som det næste.
            switch (state)
            {
                case ConversationState.WaitingForWho when IsWhoQuestion(response):
                    await session.WriteLineAsync(joke.Who, cancellationToken);
                    state = ConversationState.WaitingForWhoName;
                    break;

                case ConversationState.WaitingForWhoName when IsWhoAnswer(response, joke.Who):
                    await session.WriteLineAsync(
                        $"{joke.Punchline} Skriv 'next joke' for næste joke eller 'quit' for at afslutte.",
                        cancellationToken);
                    state = ConversationState.WaitingForNextJoke;
                    break;

                case ConversationState.WaitingForNextJoke
                    when response.Equals("next joke", StringComparison.OrdinalIgnoreCase) ||
                         response.Equals("next", StringComparison.OrdinalIgnoreCase):
                    joke = jokeSelector.SelectJoke();
                    await session.WriteLineAsync("Knock! Knock!", cancellationToken);
                    state = ConversationState.WaitingForWho;
                    break;

                case ConversationState.WaitingForNextJoke
                    when response.Equals("quit", StringComparison.OrdinalIgnoreCase):
                    await session.WriteLineAsync("Goodbye!", cancellationToken);
                    state = ConversationState.Finished;
                    break;

                case ConversationState.WaitingForWho:
                    await session.WriteLineAsync("Please respond with 'Who's there?'", cancellationToken);
                    break;

                case ConversationState.WaitingForWhoName:
                    await session.WriteLineAsync($"Please respond with '{joke.Who} who?'", cancellationToken);
                    break;

                case ConversationState.WaitingForNextJoke:
                    await session.WriteLineAsync("Skriv enten 'next joke' eller 'quit'.", cancellationToken);
                    break;
            }
        }
    }

    /// <summary>Kontrollerer de accepterede varianter af "Who's there?".</summary>
    private static bool IsWhoQuestion(string response) =>
        response.Equals("who's there?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("whos there?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("who's there", StringComparison.OrdinalIgnoreCase) ||
        response.Equals("whos there", StringComparison.OrdinalIgnoreCase);

    /// <summary>Kontrollerer om klienten svarer med jokernes navn efterfulgt af "who".</summary>
    private static bool IsWhoAnswer(string response, string who) =>
        response.Equals($"{who} who?", StringComparison.OrdinalIgnoreCase) ||
        response.Equals($"{who} who", StringComparison.OrdinalIgnoreCase);
}
