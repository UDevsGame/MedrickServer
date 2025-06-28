namespace medrick_game_server.Network;

public interface Transport
{
    Task<ConnectionResult> Connect();
    Task Disconnect();
    void SendMessage(byte[] messageToBytes);
    void RegisterReceivedMessage(Action<byte[]> OnRecievedMessage);
}

public class ConnectionResult
{
    public int NetworkId;
}