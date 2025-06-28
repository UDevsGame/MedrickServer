using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib;
public class LiteNetLibFactory : NetworkFactory
{
    public NetworkServer CreateServer()
    {
        return new LiteNetLibServer();
    }
}