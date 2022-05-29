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
                shot = ValidateShot(clientTime: reader.ReadInt64(), shot: shot);
            }
            BroadcastShot(peer, shot);
        }

        private ShotSnapshot ValidateShot(long clientTime, ShotSnapshot shot)
        {
            var targetPlayer = _remotePlayersController.GetRemotePlayer(shot.Target);
            if (targetPlayer == null) return CancelShot(shot, Vector3.zero, "target is null");

            var time = DateTime.FromBinary(clientTime);
            var offset = DateTime.UtcNow - time;
            var timeOfHitInServerTime = Time.time - offset.TotalSeconds;
            targetPlayer.StartCheck((float)timeOfHitInServerTime);

            try
            {
                Physics.Raycast(shot.Position, shot.Direction, out var hit, Config.MaxShotDistance);
                var targetPlayerCollider = targetPlayer.GetComponent<CapsuleCollider>();
                if (hit.collider == null) return CancelShot(shot, targetPlayerCollider.transform.position, "no hit");

                var target = hit.collider.gameObject;
                if (!target.CompareTag("RemotePlayer"))
                    return CancelShot(shot, targetPlayerCollider.transform.position, "hit not remote");

                var remotePlayer = target.GetComponent<RemotePlayerController>();
                if (remotePlayer.Id != shot.Target)
                    return CancelShot(shot, targetPlayerCollider.transform.position, "wrong player");

                var hitCollPosition = hit.collider.transform.position;
                Debug.Log($"shot hit at {hit.point}!");
                return new ShotSnapshot(shot.Target, hitCollPosition, shot.Direction);
                // return new ShotSnapshot(shot.Target, hit.point, shot.Direction);
            }
            finally
            {
                targetPlayer.FinishCheck();
            }
        }

        private static ShotSnapshot CancelShot(ShotSnapshot shot, Vector3 colliderPosition, string reason)
        {
            Debug.Log($"Shot canceled!!! {reason}");
            return shot.WithTarget(-1).WithPosition(colliderPosition);
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