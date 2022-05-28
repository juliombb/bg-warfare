using System;
using DefaultNamespace;
using LiteNetLib;
using LiteNetLib.Utils;
using Model;
using Serialization;
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
        private Vector3 _lastRotation = Vector3.zero;
        private NetDataWriter _writer = new NetDataWriter();
        private int _sequence = 0;
        private float _epsilon = 0.01f;
        public void Setup(GameObject baseClient, ServerData server)
        {
            _server = server.Peer;
            _monitoredTransform = baseClient.transform.Find("FPC");
        }
        private void FixedUpdate()
        {
            if (Time.time - _lastUpdate < Config.TickRate) return;
            _writer.Reset();
            var position = _monitoredTransform.position;
            var rotation = _monitoredTransform.rotation;
            if (Vector3.Distance(position, _lastPosition) < _epsilon 
                && Vector3.Distance(rotation.eulerAngles, _lastRotation) < _epsilon)
            {
                return;
            }
            _lastPosition = position;
            _lastRotation = rotation.eulerAngles;
            _writer.Put((byte)ServerCommand.PositionOfPlayer);
            PlayerSnapshotSerializer.PutPlayerSnapshot(_writer, new PlayerSnapshot(_sequence++, position, rotation));
            _server.Send(_writer, DeliveryMethod.Unreliable);
            _lastUpdate = Time.time;
        }
    }
}