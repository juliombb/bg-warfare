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
        private readonly HashSet<int> _playersToSend = new();

        public MovementServerSystem(ServerController serverController)
        {
            _serverController = serverController;
        }

        public ServerCommand CommandKey => ServerCommand.PositionOfPlayer;

        public void Handle(int playerId, byte[] data)
        {
            _lastKnownPositions[playerId] = data;
            _playersToSend.Add(playerId);
        }

        public void Poll()
        {
            var stream = new MemoryStream(_playersToSend.Count * 20);
            foreach (var playerToSend in _playersToSend)
            {
                var lastKnownPosition = _lastKnownPositions[playerToSend];
                stream.Write(BitConverter.GetBytes(playerToSend));
                stream.Write(lastKnownPosition);
            }
            _playersToSend.Clear();

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