using LiteNetLib;
using LiteNetLib.Utils;
using Model;
using Serialization;
using Server;
using UnityEngine;

namespace Client
{
    public class ClientShotMonitor: MonoBehaviour, IClientMonitor
    {
        private FirstPersonController _fpc;
        private NetPeer _server;
        private NetDataWriter _writer = new();

        public void Setup(GameObject baseClient, ServerData server)
        {
            _fpc = baseClient.GetComponentInChildren<FirstPersonController>();
            _fpc.OnShot(OnShot);
            _server = server.Peer;
        }

        private void OnShot(Vector3 position, Vector3 direction, int hitId)
        {
            _writer.Reset();
            _writer.Put((byte)ServerCommand.Shot);
            _writer.PutShotSnapshot(new ShotSnapshot(hitId, position, direction));
            if (hitId != -1) _writer.Put(System.DateTime.UtcNow.AddSeconds(-Time.deltaTime).ToBinary());
            _server.Send(_writer, hitId > 0 ? DeliveryMethod.ReliableUnordered : DeliveryMethod.Unreliable);
        }
    }
}