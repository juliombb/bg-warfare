using System;
using System.IO;
using DefaultNamespace;
using LiteNetLib;
using LiteNetLib.Utils;
using Model;
using Serialization;
using UnityEngine;

namespace Server
{
    public class ShotServerSystem: IServerSystem
    {
        public ServerCommand CommandKey => ServerCommand.Shot;

        private readonly ServerController _serverController;
        private readonly RemotePlayersController _remotePlayersController;
        private readonly NetDataWriter _writer = new NetDataWriter();

        public ShotServerSystem(ServerController serverController, GameObject baseGame)
        {
            _remotePlayersController = baseGame.GetComponentInChildren<RemotePlayersController>();
            _serverController = serverController;
        }
        public void Handle(int peer, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var shot = reader.ReadShotSnapshot();
            if (shot.Target >= 0)
            {
                shot = ValidateShot(clientSequence: reader.ReadInt32(), shot: shot);
            }
            BroadcastShot(peer, shot);
        }

        private ShotSnapshot ValidateShot(int clientSequence, ShotSnapshot shot)
        {
            var targetPlayer = _remotePlayersController.GetRemotePlayer(shot.Target);
            if (targetPlayer == null) return CancelShot(Vector3.zero, Vector3.zero, "target is null");

            var posOffset = targetPlayer.OffsetFrom(clientSequence);
            // offsetFrom = transform - rollback
            // rollback = transform - offsetFrom
            // forward(rollback) = transform = rollback + offsetFrom
            // forward(Shot) = shot + offsetFrom
            Physics.Raycast(shot.Position + posOffset, shot.Direction, out var hit, Config.MaxShotDistance);
            var targetPlayerCollider = targetPlayer.GetComponent<CapsuleCollider>();
            var initialPlayerPosition = targetPlayerCollider.transform.position;
            var rollbackPosition = initialPlayerPosition - posOffset;
            if (hit.collider == null) 
                return CancelShot(rollbackPosition, initialPlayerPosition, "no hit");

            var target = hit.collider.gameObject;
            if (!target.CompareTag("RemotePlayer"))
                return CancelShot(rollbackPosition, initialPlayerPosition, $"hit {target}");

            var remotePlayer = target.GetComponent<RemotePlayerController>();
            if (remotePlayer.Id != shot.Target)
                return CancelShot(rollbackPosition, initialPlayerPosition, "wrong player");

            Debug.Log($"shot hit at {hit.point}!");
            return new ShotSnapshot(shot.Target, rollbackPosition, initialPlayerPosition);
            // return new ShotSnapshot(shot.Target, hit.point, shot.Direction);
        }

        private static ShotSnapshot CancelShot(Vector3 colliderPosition, Vector3 initialPlayerPosition, string reason)
        {
            Debug.Log($"Shot canceled!!! {reason}");
            return new ShotSnapshot(-1, colliderPosition, initialPlayerPosition);
        }

        private void BroadcastShot(int shooter, ShotSnapshot shot)
        {
            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                _writer.Reset();
                _writer.Put((byte)ServerCommand.Shot);
                _writer.Put(shooter);
                _writer.PutShotSnapshot(shot);
                netPeer.Send(_writer, DeliveryMethod.ReliableUnordered);
            }
        }

        public void OnPeerEnter(NetPeer peer) { }

        public void Poll() { }
    }
}