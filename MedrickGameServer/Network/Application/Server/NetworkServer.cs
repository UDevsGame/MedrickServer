namespace MedrickGameServer.Network.Application;

public interface NetworkServer
{
    // State
    bool IsRunning { get; }
    IReadOnlySet<ClientId> ConnectedClients { get; }
    
    // Lifecycle
    Task StartAsync(int port);
    Task StopAsync();
    
    // Messaging
    Task SendToClientAsync(ClientId clientId, byte[] data);
    Task SendToAllClientsAsync(byte[] data);
    Task DisconnectClientAsync(ClientId clientId);
    
    // Events
    void Subscribe(NetworkEventHandler handler);
    void Unsubscribe(NetworkEventHandler handler);
}