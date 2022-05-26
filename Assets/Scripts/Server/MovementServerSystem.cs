using System;
using System.Collections.Generic;
using System.IO;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server
{
    public class MovementServerSystem: IServerSystem
    {
        private readonly ServerController _serverController;
        private readonly Dictionary<int, byte[]> _lastKnownPositions = new();

        public MovementServerSystem(ServerController serverController)
        {
            _serverController = serverController;
        }

        public ServerCommand CommandKey => ServerCommand.PositionOfPlayer;

        public void Handle(int playerId, byte[] data)
        {
            _lastKnownPositions[playerId] = data;
        }

        public void Poll()
        {
            var stream = new MemoryStream(_lastKnownPositions.Count * 20);
            foreach (var lastKnownPosition in _lastKnownPositions)
            {
                stream.Write(BitConverter.GetBytes(lastKnownPosition.Key));
                stream.Write(lastKnownPosition.Value);
            }

            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte) ServerCommand.PositionOfPlayers);
            writer.Put(stream.ToArray());
            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                netPeer.Send(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}