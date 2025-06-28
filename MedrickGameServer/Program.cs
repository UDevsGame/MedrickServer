using MedrickGameServer.Network.Application;
using MedrickGameServer.Network.Main.LiteNetLib;

namespace MedrickGameServer;

class Program
{
    static async Task Main(string[] args)
    {
        NetworkFactory networkFactory = new LiteNetLibFactory();
        NetworkServer networkServer = networkFactory.CreateServer();
        await networkServer.StartAsync(9050);
    }
}