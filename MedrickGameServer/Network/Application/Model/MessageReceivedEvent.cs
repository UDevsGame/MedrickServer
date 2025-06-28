namespace MedrickGameServer.Network.Application;

public sealed class MessageReceivedEvent
{
    public NetworkMessage Message { get; }
    
    public MessageReceivedEvent(NetworkMessage message)
    {
        Message = message;
    }
}