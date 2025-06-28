namespace MedrickGameServer.NetworkLayer;

public interface Network
{
    Task<ConnectionResult> Connect();
    Task Disconnect();
    void SendMessage(byte[] messageToBytes);
    void RegisterListener(NetworkListener networkListener);
    void UnregisterListener(NetworkListener networkListener);
}

public interface NetworkListener
{
    void OnMessageReceived(byte[] message);
}

public class ConnectionResult
{
    public string networkId;
    public string connectionStatus;
}