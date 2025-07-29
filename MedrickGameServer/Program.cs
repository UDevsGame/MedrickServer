using System.Text;
using System.Text.Json;
using MedrickGameServer.Network.Application;
using MedrickGameServer.Network.Main.LiteNetLib;
using Prometheus;

namespace MedrickGameServer;

class Program : NetworkEventHandler
{
    private static NetworkServer networkServer;
    private static readonly CancellationTokenSource cancellationTokenSource = new();
    private static MetricServer metricServer;

    private static readonly Counter ClientConnectedCounter = Metrics.CreateCounter(
        "game_server_client_connected_total", "Total number of clients that have connected");
    private static readonly Counter ClientDisconnectedCounter = Metrics.CreateCounter(
        "game_server_client_disconnected_total", "Total number of clients that have disconnected");
    private static readonly Counter MessagesReceivedCounter = Metrics.CreateCounter(
        "game_server_messages_received_total", "Total number of messages received");
    private static readonly Gauge ConnectedClientsGauge = Metrics.CreateGauge(
        "game_server_connected_clients", "Current number of connected clients");
    
    static async Task Main(string[] args)
    {
        // تنظیم handler برای Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        Program program = new Program();
        networkServer = LiteNetLibServer.CreateInstance();
        networkServer.Subscribe(program);

        metricServer = new MetricServer(port: 9100);
        metricServer.Start();
        
        try
        {
            // برای Docker باید روی 0.0.0.0 listen کند
            await networkServer.StartAsync(9050);
            Console.WriteLine("Server is running on port: 9050");
            Console.WriteLine("Press Ctrl+C to stop the server...");
            
            // نگه داشتن برنامه تا زمان دریافت signal برای توقف
            await Task.Delay(-1, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server is shutting down...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // cleanup کردن منابع
            await metricServer.StopAsync();
            Console.WriteLine("Server stopped.");
        }
    }

    public async void OnClientConnected(ClientConnectedEvent eventArgs)
    {
        Console.WriteLine($"Client Connected - ID: {eventArgs.ClientId}, EndPoint: {eventArgs.EndPoint}, Time: {eventArgs.ConnectedAt}");
        ClientConnectedCounter.Inc();
        ConnectedClientsGauge.Inc();
        await networkServer.SendToClientAsync(eventArgs.ClientId, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventArgs.ClientId)));
    }

    public void OnClientDisconnected(ClientDisconnectedEvent eventArgs)
    {
        Console.WriteLine($"Client Disconnected - ID: {eventArgs.ClientId}, Reason: {eventArgs.Reason}, Time: {eventArgs.DisconnectedAt}");
        ClientDisconnectedCounter.Inc();
        ConnectedClientsGauge.Dec();
    }

    public async void OnMessageReceived(MessageReceivedEvent eventArgs)
    {
        Console.WriteLine($"Message Received - From: {eventArgs.Message.SenderId}, Time: {eventArgs.Message.Timestamp}, Data: {Encoding.UTF8.GetString(eventArgs.Message.Data)}");
        MessagesReceivedCounter.Inc();
        await networkServer.SendToAllClientsAsync(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(eventArgs.Message)));
    }
}