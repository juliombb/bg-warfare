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
                var clientTime = DateTime.FromBinary(reader.ReadInt64());
                var ping = _serverController.Server.GetPeerById(peer).Ping;
                shot = ValidateShot(clientTime: clientTime.AddMilliseconds(-ping) , shot: shot);
            }
            BroadcastShot(peer, shot);
        }

        private ShotSnapshot ValidateShot(DateTime clientTime, ShotSnapshot shot)
        {
            var targetPlayer = _remotePlayersController.GetRemotePlayer(shot.Target);
            if (targetPlayer == null) return CancelShot(Vector3.zero, Vector3.zero, "target is null");

            var offset = DateTime.UtcNow - clientTime;
            var timeOfHitInServerTime = Time.time - offset.TotalSeconds;
            var initialPlayerPosition = targetPlayer.GetComponent<CapsuleCollider>().transform.position;
            targetPlayer.StartCheck((float)timeOfHitInServerTime);

            try
            {
                Physics.Raycast(shot.Position, shot.Direction, out var hit, Config.MaxShotDistance);
                var targetPlayerCollider = targetPlayer.GetComponent<CapsuleCollider>();
                if (hit.collider == null) 
                    return CancelShot(targetPlayerCollider.transform.position, initialPlayerPosition, "no hit");

                var target = hit.collider.gameObject;
                if (!target.CompareTag("RemotePlayer"))
                    return CancelShot(targetPlayerCollider.transform.position, initialPlayerPosition, "hit not remote");

                var remotePlayer = target.GetComponent<RemotePlayerController>();
                if (remotePlayer.Id != shot.Target)
                    return CancelShot(targetPlayerCollider.transform.position, initialPlayerPosition, "wrong player");

                var hitCollPosition = hit.collider.transform.position;
                Debug.Log($"shot hit at {hit.point}!");
                return new ShotSnapshot(shot.Target, hitCollPosition, initialPlayerPosition);
                // return new ShotSnapshot(shot.Target, hit.point, shot.Direction);
            }
            finally
            {
                targetPlayer.FinishCheck();
            }
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