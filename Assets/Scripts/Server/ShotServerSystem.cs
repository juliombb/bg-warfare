using System.IO;
using LiteNetLib;
using LiteNetLib.Utils;
using Serialization;
using UnityEngine;

namespace Server
{
    public class ShotServerSystem: IServerSystem
    {
        public ServerCommand CommandKey => ServerCommand.Shot;

        private readonly ServerController _serverController;
        private readonly RemotePlayersController _remotePlayersController;
        private readonly NetDataWriter _writer = new NetDataWriter();

        public ShotServerSystem(ServerController serverController, GameObject baseGame)
        {
            _remotePlayersController = baseGame.GetComponentInChildren<RemotePlayersController>();
            _serverController = serverController;
        }
        public void Handle(int peer, byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            var shot = reader.ReadShotSnapshot();
            if (shot.Target > 0)
            {
                // todo hit validation
            }
            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                _writer.Reset();
                _writer.Put((byte)ServerCommand.Shot);
                _writer.Put(peer);
                _writer.PutShotSnapshot(shot);
                netPeer.Send(_writer, shot.Target > 0 ? DeliveryMethod.ReliableUnordered : DeliveryMethod.Unreliable);
            }
        }

        public void OnPeerEnter(NetPeer peer) { }

        public void Poll() { }
    }
}