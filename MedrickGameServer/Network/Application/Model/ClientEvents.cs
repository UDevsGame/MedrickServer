namespace MedrickGameServer.Network.Application;

public readonly struct ClientConnectedEvent
{
    public ClientId ClientId { get; }
    public string EndPoint { get; }
    public DateTime ConnectedAt { get; }
    
    public ClientConnectedEvent(ClientId clientId, string endPoint, DateTime connectedAt)
    {
        ClientId = clientId;
        EndPoint = endPoint;
        ConnectedAt = connectedAt;
    }
}

public readonly struct ClientDisconnectedEvent
{
    public ClientId ClientId { get; }
    public DisconnectedReason Reason { get; }
    public DateTime DisconnectedAt { get; }
    
    public ClientDisconnectedEvent(ClientId clientId, DisconnectedReason reason, DateTime disconnectedAt)
    {
        ClientId = clientId;
        Reason = reason;
        DisconnectedAt = disconnectedAt;
    }
}

[Flags]
public enum DisconnectedReason : byte
{
    UserRequested = 1 << 0,  // 0x01
    Timeout = 1 << 1,        // 0x02
    ConnectionLost = 1 << 2, // 0x04
    ServerShutdown = 1 << 3, // 0x08
    Kicked = 1 << 4          // 0x10
}