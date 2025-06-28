using MedrickGameServer.Network.Application;
using MedrickGameServer.Network.Main.LiteNetLib;

namespace MedrickGameServer.NetworkLayer.Main;

public class LiteNetLibFactory : NetworkFactory
{
    public NetworkServer CreateServer()
    {
        return new LiteNetLibServer();
    }
}