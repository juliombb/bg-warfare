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
            if (shot.Target > 0)
            {
                shot = ValidateShot(clientTime: reader.ReadInt64(), shot: shot);
            }
            BroadcastShot(peer, shot);
        }

        private ShotSnapshot ValidateShot(long clientTime, ShotSnapshot shot)
        {
            var targetPlayer = _remotePlayersController.GetRemotePlayer(shot.Target);
            if (targetPlayer == null) return CancelShot(shot);

            var time = DateTime.FromBinary(clientTime);
            var offset = DateTime.UtcNow - time;
            var timeOfHitInServerTime = Time.time - offset.TotalSeconds;
            targetPlayer.StartCheck((float)timeOfHitInServerTime);
            
            Physics.Raycast(shot.Position, shot.Direction, out var hit, Config.MaxShotDistance);
            if (hit.collider == null) return CancelShot(shot);

            var target = hit.collider.gameObject;
            if (!target.CompareTag("RemotePlayer")) return CancelShot(shot);

            var remotePlayer = target.GetComponent<RemotePlayerController>();
            if (remotePlayer.Id != shot.Target) return CancelShot(shot);
            Debug.Log("shot hit!");

            targetPlayer.FinishCheck();
            return new ShotSnapshot(shot.Target, hit.collider.transform.position, shot.Direction);
            // return new ShotSnapshot(shot.Target, hit.point, shot.Direction);
        }

        private static ShotSnapshot CancelShot(ShotSnapshot shot)
        {
            Debug.Log("Shot canceled!!!");
            return shot.WithTarget(-1);
        }

        private void BroadcastShot(int shooter, ShotSnapshot shot)
        {
            foreach (var netPeer in _serverController.Server.ConnectedPeerList)
            {
                _writer.Reset();
                _writer.Put((byte)ServerCommand.Shot);
                _writer.Put(shooter);
                _writer.PutShotSnapshot(shot);
                netPeer.Send(_writer, shot.Target > 0 ? DeliveryMethod.ReliableUnordered : DeliveryMethod.Unreliable);
            }
        }

        public void OnPeerEnter(NetPeer peer) { }

        public void Poll() { }
    }
}