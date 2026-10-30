namespace PingPongServer.Domain;

public enum ConversationState
{
    WaitingForWho,
    WaitingForWhoName,
    WaitingForNextJoke,
    Finished
}
