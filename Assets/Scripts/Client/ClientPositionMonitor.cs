using System;
using DefaultNamespace;
using LiteNetLib;
using LiteNetLib.Utils;
using Server;
using UnityEngine;

namespace Client
{
    public class ClientPositionMonitor : MonoBehaviour, IClientMonitor
    {
        private float _lastUpdate = 0f;
        private NetPeer _server;
        private Transform _monitoredTransform;
        private Vector3 _lastPosition = Vector3.zero;
        private Quaternion _lastRotation = Quaternion.identity;
        private NetDataWriter _writer = new NetDataWriter();
        private int sequence = 0;
        public void Setup(GameObject baseClient, ServerData server)
        {
            _server = server.Peer;
            _monitoredTransform = baseClient.transform.Find("FPC");
        }
        private void Update()
        {
            if (Time.time - _lastUpdate < Config.TickRate) return;
            _writer.Reset();
            var position = _monitoredTransform.position;
            var rotation = _monitoredTransform.rotation;
            if (Vector3.Distance(position, _lastPosition) < Vector3.kEpsilon && _lastRotation == rotation)
            {
                return;
            }
            _writer.Put((byte)ServerCommand.PositionOfPlayer);
            _writer.Put(BitConverter.GetBytes(sequence++), 0 , 4);
            _writer.Put(BitConverter.GetBytes(position.x), 0 , 4);
            _writer.Put(BitConverter.GetBytes(position.y), 0 , 4);
            _writer.Put(BitConverter.GetBytes(position.z), 0 , 4);
            _writer.Put(BitConverter.GetBytes(rotation.eulerAngles.y), 0 , 4);
            _server.Send(_writer, DeliveryMethod.Unreliable);
            _lastUpdate = Time.time;
        }
    }
}