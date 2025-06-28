using LiteNetLib;
using MedrickGameServer.Network.Application;
namespace MedrickGameServer.Network.Main.LiteNetLib;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;


public class LiteNetLibServer : NetworkServer, INetEventListener
{
    private NetManager _server;
    private readonly ConcurrentDictionary<int, ClientId> _peerToClientMap;
    private readonly ConcurrentDictionary<ClientId, NetPeer> _clientToPeerMap;
    private readonly ConcurrentBag<NetworkEventHandler> _eventHandlers;
    private bool _isRunning;
    private readonly object _lockObject = new object();

    public bool IsRunning => _isRunning;

    public IReadOnlySet<ClientId> ConnectedClients => 
        _clientToPeerMap.Keys.ToHashSet();

    public LiteNetLibServer()
    {
        _peerToClientMap = new ConcurrentDictionary<int, ClientId>();
        _clientToPeerMap = new ConcurrentDictionary<ClientId, NetPeer>();
        _eventHandlers = new ConcurrentBag<NetworkEventHandler>();
    }

    public async Task StartAsync(int port)
    {
        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    throw new InvalidOperationException("Server is already running");

                _server = new NetManager(this)
                {
                    BroadcastReceiveEnabled = true,
                    UnconnectedMessagesEnabled = true,
                    UpdateTime = 15,
                    PingInterval = 1000
                };

                bool started = _server.Start(port);
                if (!started)
                    throw new InvalidOperationException($"Failed to start server on port {port}");

                _isRunning = true;

                // Start polling task
                _ = Task.Run(async () =>
                {
                    while (_isRunning)
                    {
                        _server.PollEvents();
                        await Task.Delay(15);
                    }
                });
            }
        });
    }

    public async Task StopAsync()
    {
        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;

                // Disconnect all clients with ServerShutdown reason
                foreach (var kvp in _clientToPeerMap.ToArray())
                {
                    var clientId = kvp.Key;
                    var peer = kvp.Value;
                    
                    peer.Disconnect();
                    
                    // Manually trigger disconnect event for server shutdown
                    var shutdownEvent = new ClientDisconnectedEvent(
                        clientId, 
                        DisconnectedReason.ServerShutdown, 
                        DateTime.UtcNow
                    );
                    NotifyEventHandlers(handler => handler.OnClientDisconnected(shutdownEvent));
                }

                _peerToClientMap.Clear();
                _clientToPeerMap.Clear();
                _server?.Stop();
            }
        });
    }

    public async Task SendToClientAsync(ClientId clientId, byte[] data)
    {
        await Task.Run(() =>
        {
            if (_clientToPeerMap.TryGetValue(clientId, out NetPeer peer))
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    peer.Send(data, DeliveryMethod.ReliableOrdered);
                }
            }
        });
    }

    public async Task SendToAllClientsAsync(byte[] data)
    {
        await Task.Run(() =>
        {
            foreach (var peer in _clientToPeerMap.Values)
            {
                if (peer.ConnectionState == ConnectionState.Connected)
                {
                    peer.Send(data, DeliveryMethod.ReliableOrdered);
                }
            }
        });
    }

    public async Task DisconnectClientAsync(ClientId clientId)
    {
        await Task.Run(() =>
        {
            if (_clientToPeerMap.TryGetValue(clientId, out NetPeer peer))
            {
                peer.Disconnect();
                // The disconnect event will be triggered by OnPeerDisconnected
            }
        });
    }

    public void Subscribe(NetworkEventHandler handler)
    {
        if (handler != null)
        {
            _eventHandlers.Add(handler);
        }
    }

    public void Unsubscribe(NetworkEventHandler handler)
    {
        // Note: ConcurrentBag doesn't support removal efficiently
        // For production, consider using a different collection
        // This is a limitation we could improve later
    }

    // === INetEventListener Implementation (LiteNetLib callbacks) ===

    public void OnPeerConnected(NetPeer peer)
    {
        var clientId = new ClientId(Guid.NewGuid().ToString());
        
        _peerToClientMap.TryAdd(peer.Id, clientId);
        _clientToPeerMap.TryAdd(clientId, peer);

        var connectEvent = new ClientConnectedEvent(
            clientId, 
            peer.Address.ToString(), 
            DateTime.UtcNow
        );

        NotifyEventHandlers(handler => handler.OnClientConnected(connectEvent));
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (_peerToClientMap.TryRemove(peer.Id, out var clientId))
        {
            _clientToPeerMap.TryRemove(clientId, out _);

            var reason = MapLiteNetDisconnectReason(disconnectInfo.Reason);
            var disconnectEvent = new ClientDisconnectedEvent(
                clientId, 
                reason, 
                DateTime.UtcNow
            );

            NotifyEventHandlers(handler => handler.OnClientDisconnected(disconnectEvent));
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            if (_peerToClientMap.TryGetValue(peer.Id, out var clientId))
            {
                byte[] data = reader.GetRemainingBytes();
                var message = new NetworkMessage(data, clientId, DateTime.UtcNow);
                var messageEvent = new MessageReceivedEvent(message);

                NotifyEventHandlers(handler => handler.OnMessageReceived(messageEvent));
            }
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

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        // Could expose this through events if needed
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        // Auto-accept all connections for now
        // Could add connection validation logic here
        request.Accept();
    }

    // === Helper Methods ===

    private DisconnectedReason MapLiteNetDisconnectReason(DisconnectReason liteNetReason)
    {
        return liteNetReason switch
        {
            DisconnectReason.DisconnectPeerCalled => DisconnectedReason.UserRequested,
            DisconnectReason.Timeout => DisconnectedReason.Timeout,
            DisconnectReason.ConnectionFailed => DisconnectedReason.ConnectionLost,
            DisconnectReason.RemoteConnectionClose => DisconnectedReason.ConnectionLost,
            _ => DisconnectedReason.ConnectionLost
        };
    }

    private void NotifyEventHandlers(Action<NetworkEventHandler> action)
    {
        foreach (var handler in _eventHandlers)
        {
            try
            {
                action(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in network event handler: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        StopAsync().Wait(1000);
    }
}