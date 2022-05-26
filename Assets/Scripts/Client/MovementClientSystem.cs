using System;
using System.IO;
using DefaultNamespace;
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
                var snapshot = new PlayerSnapshot(
                    sequence: stream.ReadInt32(),
                    position: new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                    rotation: Quaternion.Euler(0, stream.ReadSingle(), 0)
                );
                if (_clientPeerId == player)
                {
                    continue;
                }
                _remotePlayersController.OnPositionUpdate(player, snapshot);
            }
        }
        
    }
}