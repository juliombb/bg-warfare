using LiteNetLib;

namespace Server
{
    public interface IServerSystem
    {
        public ServerCommand CommandKey { get; }

        void Handle(int peer, byte[] data);
        void OnPeerEnter(NetPeer peer);
        void OnPeerDisconnected(NetPeer peer);
        void Poll();
    }
}