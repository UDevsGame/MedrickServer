using LiteNetLib;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Network.Main.LiteNetLib.Utils;

internal static class ClientIdGenerator
{
    public static ClientId Generate() => new(Guid.NewGuid().ToString());
}

internal static class DisconnectReasonMapper
{
    public static DisconnectedReason MapFromLiteNet(DisconnectReason liteNetReason)
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
}