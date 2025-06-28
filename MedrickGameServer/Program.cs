using MedrickGameServer.Network.Application;
using MedrickGameServer.Network.Main.LiteNetLib;

namespace MedrickGameServer;

class Program
{
    static async Task Main(string[] args)
    {
        NetworkServer networkServer = LiteNetLibServer.CreateInstance();
        await networkServer.StartAsync(9050);
    }
}