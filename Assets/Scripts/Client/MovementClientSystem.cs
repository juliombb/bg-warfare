using System;
using System.IO;
using DefaultNamespace;
using Server;
using UnityEngine;

namespace Client
{
    public class MovementClientSystem : IClientSystem
    {
        private RemotePlayersController _remotePlayersController;
        ServerCommand IClientSystem.CommandKey => ServerCommand.PositionOfPlayers;

        void IClientSystem.Install(GameObject baseClient)
        {
            _remotePlayersController = baseClient.GetComponentInChildren<RemotePlayersController>();
        }

        void IClientSystem.Handle(byte[] data)
        {
            using var stream = new BinaryReader(new MemoryStream(data));
            while (stream.HasNext())
            {
                var player = BitConverter.ToInt32(stream.ReadBytes(4));
                var snapshot = new PlayerSnapshot(
                    sequence: stream.ReadInt32(),
                    position: new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                    rotation: Quaternion.Euler(0, stream.ReadSingle(), 0)
                );
                _remotePlayersController.OnPositionUpdate(player, snapshot);
            }
        }
        
    }
}