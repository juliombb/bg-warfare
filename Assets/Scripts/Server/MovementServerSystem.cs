using System;
using System.Collections.Generic;
using System.IO;
using LiteNetLib;
using LiteNetLib.Utils;
using Model;
using Serialization;
using UnityEngine;

namespace Server
{
    public class MovementServerSystem: IServerSystem
    {
        private readonly ServerController _serverController;
        private readonly Dictionary<int, PlayerSnapshot> _lastKnownPositions = new();
        private readonly HashSet<int> _playersToSend = new();
        private readonly NetDataWriter _writer = new NetDataWriter();
        private readonly RemotePlayersController _remotePlayersController;

        public MovementServerSystem(ServerController serverController, GameObject baseGame)
        {
            _serverController = serverController;
            _remotePlayersController = baseGame.GetComponentInChildren<RemotePlayersController>();
        }

        public ServerCommand CommandKey => ServerCommand.PositionOfPlayer;

        public void Handle(int playerId, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var snapshot = reader.ReadPlayerSnapshot();
            _lastKnownPositions[playerId] = snapshot; 
            _playersToSend.Add(playerId);
            _remotePlayersController.OnPositionUpdate(playerId, snapshot);
        }

        public void OnPeerEnter(NetPeer peer)
        {
            _writer.Reset();
            _writer.Put((byte) ServerCommand.PositionOfPlayers);
            foreach (var playerToSend in _lastKnownPositions)
            {
                _writer.Put(BitConverter.GetBytes(playerToSend.Key));
                _writer.PutPlayerSnapshot(playerToSend.Value);
            }

            Debug.Log("sending position of players to peer");
            peer.Send(_writer, DeliveryMethod.ReliableOrdered);
        }

        public void OnPeerDisconnected(NetPeer peer)
        {
            _lastKnownPositions.Remove(peer.Id);
            _writer.Reset();
            _writer.Put((byte) ServerCommand.OtherPlayerDisconnected);
            _writer.Put(peer.Id);
            Debug.Log($"peer {peer.Id} disconnected");

            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                netPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public void Poll()
        {
            if (_playersToSend.Count == 0) return;
            _writer.Reset();
            _writer.Put((byte) ServerCommand.PositionOfPlayers);
            foreach (var playerToSend in _playersToSend)
            {
                if (!_lastKnownPositions.ContainsKey(playerToSend)) continue;
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