using System.Collections.Concurrent;
using LiteNetLib;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib;

internal class ClientRepository
{
    private readonly ConcurrentDictionary<int, ClientId> peerToClientMap = new();
    private readonly ConcurrentDictionary<ClientId, NetPeer> clientToPeerMap = new();

    public void AddClient(ClientId clientId, NetPeer peer)
    {
        peerToClientMap.TryAdd(peer.Id, clientId);
        clientToPeerMap.TryAdd(clientId, peer);
    }

    public void RemoveClient(ClientId clientId)
    {
        if (clientToPeerMap.TryRemove(clientId, out var peer))
        {
            peerToClientMap.TryRemove(peer.Id, out _);
        }
    }

    public ClientId? GetClientForPeer(int peerId)
    {
        return peerToClientMap.TryGetValue(peerId, out var clientId) ? clientId : null;
    }

    public NetPeer GetPeerForClient(ClientId clientId)
    {
        return clientToPeerMap.TryGetValue(clientId, out var peer) ? peer : null;
    }

    public IReadOnlySet<ClientId> GetConnectedClients() => 
        clientToPeerMap.Keys.ToHashSet();

    public IEnumerable<(ClientId ClientId, NetPeer Peer)> GetAllClientPeers() => 
        clientToPeerMap.Select(kvp => (kvp.Key, kvp.Value));

    public IEnumerable<NetPeer> GetConnectedPeers() => 
        clientToPeerMap.Values.Where(peer => peer.ConnectionState == ConnectionState.Connected);

    public void ClearAll()
    {
        peerToClientMap.Clear();
        clientToPeerMap.Clear();
    }
}