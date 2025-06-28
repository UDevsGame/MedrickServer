namespace MedrickGameServer.Network.Application;

public interface NetworkEventHandler
{
    void OnClientConnected(ClientConnectedEvent eventArgs);
    void OnClientDisconnected(ClientDisconnectedEvent eventArgs);
    void OnMessageReceived(MessageReceivedEvent eventArgs);
}