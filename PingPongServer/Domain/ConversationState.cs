namespace PingPongServer.Domain;

/// <summary>Angiver hvilket svar serveren venter på i samtalen.</summary>
public enum ConversationState
{
    WaitingForWho,
    WaitingForWhoName,
    WaitingForNextJoke,
    Finished
}
