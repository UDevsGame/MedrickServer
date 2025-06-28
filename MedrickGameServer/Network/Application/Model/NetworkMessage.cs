namespace MedrickGameServer.Network.Application;

public readonly struct NetworkMessage
{
    public byte[] Data { get; }
    public ClientId SenderId { get; }
    public DateTime Timestamp { get; }
    
    public NetworkMessage(byte[] data, ClientId senderId, DateTime timestamp)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        SenderId = senderId;
        Timestamp = timestamp;
    }
}