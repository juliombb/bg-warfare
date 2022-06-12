using System;
using System.IO;
using DefaultNamespace;
using LiteNetLib;
using Serialization;
using Server;
using TMPro;
using UnityEngine;

namespace Client
{
    public class ShotClientSystem : IClientSystem
    {
        ServerCommand IClientSystem.CommandKey => ServerCommand.Shot;

        private RemotePlayersController _remotePlayersController;
        private FirstPersonController _firstPersonController;
        private TMP_Text _hitRate;
        private int _clientPeerId;
        private float hits = 0;
        private float shots = 0;

        void IClientSystem.Install(GameObject baseClient, ServerData server)
        {
            _remotePlayersController = baseClient.GetComponentInChildren<RemotePlayersController>();
            _firstPersonController = baseClient.GetComponentInChildren<FirstPersonController>();
            _hitRate = GameObject.Find("HitRateText").GetComponent<TMP_Text>();
            _clientPeerId = server.ClientPeerId;
        }

        void IClientSystem.Handle(byte[] data)
        {
            using var stream = new BinaryReader(new MemoryStream(data));
            var fromPlayer = stream.ReadInt32();
            var shot = stream.ReadShotSnapshot();
            _remotePlayersController.OnShot(fromPlayer);

            if (fromPlayer == _clientPeerId)
            {
                if (_firstPersonController.RenderCapsule(shot.Position, 0))
                {
                    _firstPersonController.RenderCapsule(shot.Direction, 1);
                }
            }

            if (shot.Target < 0)
            {
                if (shot.Target == -2)
                {
                    shots++;
                    UpdateHitRate();
                }
                return;
            }

            if (fromPlayer == _clientPeerId)
            {
                shots++;
                hits++;
                UpdateHitRate();
            }

            if (shot.Target == _clientPeerId)
            {
                _firstPersonController.TakeShot();
                return;
            }

            _remotePlayersController.TakeShot(shot.Target, shot.Position, shot.Direction);
            
        }

        private void UpdateHitRate()
        {
            _hitRate.text = $"Hit rate: {hits / shots}";
        }
    }
}