using System;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using DefaultNamespace.Serialization;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server
{
    public class MovementServerSystem: IServerSystem
    {
        private readonly ServerController _serverController;
        private readonly Dictionary<int, PlayerSnapshot> _lastKnownPositions = new();
        private readonly HashSet<int> _playersToSend = new();
        private readonly NetDataWriter _writer = new NetDataWriter();

        public MovementServerSystem(ServerController serverController)
        {
            _serverController = serverController;
        }

        public ServerCommand CommandKey => ServerCommand.PositionOfPlayer;

        public void Handle(int playerId, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            _lastKnownPositions[playerId] = reader.ReadPlayerSnapshot();
            _playersToSend.Add(playerId);
        }

        public void Poll()
        {
            _writer.Reset();
            _writer.Put((byte) ServerCommand.PositionOfPlayers);
            foreach (var playerToSend in _playersToSend)
            {
                var lastKnownPosition = _lastKnownPositions[playerToSend];
                _writer.Put(BitConverter.GetBytes(playerToSend));
                _writer.PutPlayerSnapshot(lastKnownPosition);
            }
            _playersToSend.Clear();

            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                netPeer.Send(_writer, DeliveryMethod.Unreliable);
            }
        }
    }
}