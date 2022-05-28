using System;
using System.IO;
using DefaultNamespace;
using DefaultNamespace.Serialization;
using LiteNetLib;
using Server;
using UnityEngine;

namespace Client
{
    public class MovementClientSystem : IClientSystem
    {
        ServerCommand IClientSystem.CommandKey => ServerCommand.PositionOfPlayers;

        private RemotePlayersController _remotePlayersController;
        private int _clientPeerId = 0;

        void IClientSystem.Install(GameObject baseClient, ServerData server)
        {
            _remotePlayersController = baseClient.GetComponentInChildren<RemotePlayersController>();
            _clientPeerId = server.ClientPeerId;
        }

        void IClientSystem.Handle(byte[] data)
        {
            using var stream = new BinaryReader(new MemoryStream(data));
            while (stream.HasNext())
            {
                var player = stream.ReadInt32();
                var snapshot = stream.ReadPlayerSnapshot();
                if (_clientPeerId == player)
                {
                    continue;
                }
                _remotePlayersController.OnPositionUpdate(player, snapshot);
                // Debug.Log($"Received remote player position ${snapshot.Position.ToString()}");
            }
        }
        
    }
}