using LiteNetLib;

namespace Client
{
    public readonly struct ServerData
    {
        public readonly NetPeer Peer;
        public readonly int ClientPeerId;

        public ServerData(NetPeer peer, int clientPeerId)
        {
            Peer = peer;
            ClientPeerId = clientPeerId;
        }
    }
}