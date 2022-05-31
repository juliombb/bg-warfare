using System;
using System.IO;
using DefaultNamespace;
using LiteNetLib;
using Serialization;
using Server;
using UnityEngine;

namespace Client
{
    public class PeerDisconnectClientSystem : IClientSystem
    {
        ServerCommand IClientSystem.CommandKey => ServerCommand.OtherPlayerDisconnected;

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
            var player = stream.ReadInt32();
            Debug.Log($"Player {player} left");
            _remotePlayersController.OnPlayerLeft(player);
        }
        
    }
}