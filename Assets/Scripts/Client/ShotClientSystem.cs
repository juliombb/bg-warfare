using System;
using System.IO;
using DefaultNamespace;
using LiteNetLib;
using Serialization;
using Server;
using UnityEngine;

namespace Client
{
    public class ShotClientSystem : IClientSystem
    {
        ServerCommand IClientSystem.CommandKey => ServerCommand.Shot;

        private RemotePlayersController _remotePlayersController;
        private FirstPersonController _firstPersonController;
        private int _clientPeerId;

        void IClientSystem.Install(GameObject baseClient, ServerData server)
        {
            _remotePlayersController = baseClient.GetComponentInChildren<RemotePlayersController>();
            _firstPersonController = baseClient.GetComponentInChildren<FirstPersonController>();
            _clientPeerId = server.ClientPeerId;
        }

        void IClientSystem.Handle(byte[] data)
        {
            using var stream = new BinaryReader(new MemoryStream(data));
            var fromPlayer = stream.ReadInt32();
            var shot = stream.ReadShotSnapshot();
            _remotePlayersController.OnShot(fromPlayer);
            if (shot.Target < 0)
            {
                return;
            }

            if (shot.Target == _clientPeerId)
            {
                _firstPersonController.TakeShot();
                return;
            }

            if (fromPlayer == _clientPeerId)
            {
                _firstPersonController.RenderCapsule(shot.Position);
            }
            _remotePlayersController.TakeShot(shot.Target, shot.Position, shot.Direction);
            
        }
        
    }
}