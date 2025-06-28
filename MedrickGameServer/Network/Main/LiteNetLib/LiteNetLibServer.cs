using LiteNetLib;
using MedrickGameServer.Network.Application;
using System.Net;
using System.Net.Sockets;
using MedrickGameServer.Network.Main.LiteNetLib.Utils;

namespace MedrickGameServer.Network.Main.LiteNetLib;

public class LiteNetLibServer : NetworkServer, INetEventListener
{
    // === Core Components ===
    private readonly ClientRepository clientRepository;
    private readonly EventDispatcher eventDispatcher;
    private readonly MessageProcessor messageProcessor;
    private readonly ServerLifecycle serverLifecycle;

    // === Public Properties ===
    public bool IsRunning => serverLifecycle.IsRunning;
    public IReadOnlySet<ClientId> ConnectedClients => clientRepository.GetConnectedClients();

    internal LiteNetLibServer()
    {
        clientRepository = new ClientRepository();
        eventDispatcher = new EventDispatcher();
        messageProcessor = new MessageProcessor(clientRepository, eventDispatcher);
        serverLifecycle = new ServerLifecycle(this, clientRepository, eventDispatcher);
    }

    public async Task StartAsync(int port) => 
        await serverLifecycle.StartAsync(port);

    public async Task StopAsync() => 
        await serverLifecycle.StopAsync();

    public async Task SendToClientAsync(ClientId clientId, byte[] data) => 
        await messageProcessor.SendToClientAsync(clientId, data);

    public async Task SendToAllClientsAsync(byte[] data) => 
        await messageProcessor.SendToAllClientsAsync(data);

    public async Task DisconnectClientAsync(ClientId clientId) => 
        await messageProcessor.DisconnectClientAsync(clientId);

    public void Subscribe(NetworkEventHandler handler) => 
        eventDispatcher.Subscribe(handler);

    public void Unsubscribe(NetworkEventHandler handler) => 
        eventDispatcher.Unsubscribe(handler);

    // === LiteNetLib Event Handlers (Delegates to Components) ===

    public void OnPeerConnected(NetPeer peer)
    {
        var clientId = ClientIdGenerator.Generate();
        clientRepository.AddClient(clientId, peer);
        eventDispatcher.NotifyClientConnected(clientId, peer.Address.ToString());
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var clientId = clientRepository.GetClientForPeer(peer.Id);
        if (!clientId.HasValue) return;

        clientRepository.RemoveClient(clientId.Value);
        var reason = DisconnectReasonMapper.MapFromLiteNet(disconnectInfo.Reason);
        eventDispatcher.NotifyClientDisconnected(clientId.Value, reason);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            messageProcessor.ProcessIncomingMessage(peer, reader);
        }
        finally
        {
            reader.Recycle();
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.WriteLine($"LiteNetLib Network error from {endPoint}: {socketError}");
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        reader.Recycle();
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        // Auto-accept all connections for now
        request.Accept();
    }

    public void Dispose()
    {
        StopAsync().Wait(1000);
    }
}