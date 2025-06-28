using LiteNetLib;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib;

internal class ServerLifecycle
{
    private const int NetworkTick = 15;
    private const int PingInterval = 1000;

    private readonly INetEventListener eventListener;
    private readonly ClientRepository clientRepository;
    private readonly EventDispatcher eventDispatcher;

    private NetManager server;
    private bool isRunning;
    private readonly object lockObject = new();

    public bool IsRunning => isRunning;

    public ServerLifecycle(INetEventListener eventListener, ClientRepository clientRepository, EventDispatcher eventDispatcher)
    {
        this.eventListener = eventListener;
        this.clientRepository = clientRepository;
        this.eventDispatcher = eventDispatcher;
    }

    public async Task StartAsync(int port)
    {
        await Task.Run(() =>
        {
            lock (lockObject)
            {
                if (isRunning)
                    throw new InvalidOperationException("Server is already running");

                CreateAndStartServer(port);
                StartPollingLoop();
                isRunning = true;
            }
        });
    }

    public async Task StopAsync()
    {
        await Task.Run(() =>
        {
            lock (lockObject)
            {
                if (!isRunning) return;

                isRunning = false;
                DisconnectAllClients();
                StopServer();
            }
        });
    }

    private void CreateAndStartServer(int port)
    {
        server = new NetManager(eventListener)
        {
            BroadcastReceiveEnabled = true,
            UnconnectedMessagesEnabled = true,
            UpdateTime = NetworkTick,
            PingInterval = PingInterval
        };

        var started = server.Start(port);
        if (!started)
            throw new InvalidOperationException($"Failed to start server on port {port}");
    }

    private void StartPollingLoop()
    {
        _ = Task.Run(async () =>
        {
            while (isRunning)
            {
                server.PollEvents();
                await Task.Delay(NetworkTick);
            }
        });
    }

    private void DisconnectAllClients()
    {
        foreach (var (clientId, peer) in clientRepository.GetAllClientPeers())
        {
            peer.Disconnect();
            eventDispatcher.NotifyClientDisconnected(clientId, DisconnectedReason.ServerShutdown);
        }
        clientRepository.ClearAll();
    }

    private void StopServer()
    {
        server?.Stop();
        server = null;
    }
}