namespace MedrickGameServer.Network.Application;

public enum DisconnectedReason
{
    UserRequested,
    Timeout,
    ConnectionLost,
    ServerShutdown,
    Kicked
}

public sealed class ClientConnectedEvent
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

public sealed class ClientDisconnectedEvent
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