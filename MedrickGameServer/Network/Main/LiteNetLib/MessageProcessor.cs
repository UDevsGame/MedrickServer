using LiteNetLib;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib;

internal class MessageProcessor
{
    private readonly ClientRepository clientRepository;
    private readonly EventDispatcher eventDispatcher;

    public MessageProcessor(ClientRepository clientRepository, EventDispatcher eventDispatcher)
    {
        this.clientRepository = clientRepository;
        this.eventDispatcher = eventDispatcher;
    }

    public async Task SendToClientAsync(ClientId clientId, byte[] data)
    {
        await Task.Run(() =>
        {
            var peer = clientRepository.GetPeerForClient(clientId);
            if (peer != null && IsConnected(peer))
            {
                peer.Send(data, DeliveryMethod.ReliableOrdered);
            }
        });
    }

    public async Task SendToAllClientsAsync(byte[] data)
    {
        await Task.Run(() =>
        {
            foreach (var peer in clientRepository.GetConnectedPeers())
            {
                peer.Send(data, DeliveryMethod.ReliableOrdered);
            }
        });
    }

    public async Task DisconnectClientAsync(ClientId clientId)
    {
        await Task.Run(() =>
        {
            var peer = clientRepository.GetPeerForClient(clientId);
            peer?.Disconnect();
        });
    }

    public void ProcessIncomingMessage(NetPeer peer, NetPacketReader reader)
    {
        var clientId = clientRepository.GetClientForPeer(peer.Id);
        if (!clientId.HasValue) return;

        var data = reader.GetRemainingBytes();
        var message = new NetworkMessage(data, clientId.Value, DateTime.UtcNow);
        eventDispatcher.NotifyMessageReceived(message);
    }

    private static bool IsConnected(NetPeer peer) => 
        peer.ConnectionState == ConnectionState.Connected;
}