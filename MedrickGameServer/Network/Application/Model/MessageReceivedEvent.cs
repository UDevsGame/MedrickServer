namespace MedrickGameServer.Network.Application;

public readonly struct MessageReceivedEvent
{
    public NetworkMessage Message { get; }
    
    public MessageReceivedEvent(NetworkMessage message)
    {
        Message = message;
    }
}