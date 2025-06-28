using System.Collections.Concurrent;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib;

internal class EventDispatcher
{
    private readonly ConcurrentBag<NetworkEventHandler> eventHandlers = new();

    public void Subscribe(NetworkEventHandler handler)
    {
        if (handler != null)
            eventHandlers.Add(handler);
    }

    public void Unsubscribe(NetworkEventHandler handler)
    {
        // Note: ConcurrentBag limitation - consider ConcurrentDictionary for production
    }

    public void NotifyClientConnected(ClientId clientId, string endPoint)
    {
        var eventArgs = new ClientConnectedEvent(clientId, endPoint, DateTime.UtcNow);
        NotifyEventHandlers(handler => handler.OnClientConnected(eventArgs));
    }

    public void NotifyClientDisconnected(ClientId clientId, DisconnectedReason reason)
    {
        var eventArgs = new ClientDisconnectedEvent(clientId, reason, DateTime.UtcNow);
        NotifyEventHandlers(handler => handler.OnClientDisconnected(eventArgs));
    }

    public void NotifyMessageReceived(NetworkMessage message)
    {
        var eventArgs = new MessageReceivedEvent(message);
        NotifyEventHandlers(handler => handler.OnMessageReceived(eventArgs));
    }

    private void NotifyEventHandlers(Action<NetworkEventHandler> action)
    {
        foreach (var handler in eventHandlers)
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
}