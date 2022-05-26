using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Client;
using LiteNetLib;
using LiteNetLib.Utils;
using Server;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class ClientConnectionController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        private NetManager _client;
        private Dictionary<byte, IClientSystem> _handlers = new();

        private void AddSystem(IClientSystem system)
        {
            _handlers.Add((byte)system.CommandKey, system);
        }

        public void OnConnect(GameObject baseClient)
        {
            if (_listener == null || _client == null)
            {
                _listener = new EventBasedNetListener();
                _client = new NetManager(_listener);
            }
            _client.Start();
            _client.Connect("localhost" /* host ip or name */, 9050 /* port */, "BgWarfare" /* text key or NetDataWriter */);
            _listener.PeerConnectedEvent += netPeer =>
            {
                SetupServer(baseClient, netPeer);
            };
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var command = dataReader.GetByte();
                if (_handlers.ContainsKey(command))
                {
                    _handlers[command].Handle(dataReader.GetRemainingBytes());
                }
                dataReader.Recycle();
            };

            StartCoroutine(Polling());
        }

        private void SetupServer(GameObject baseClient, NetPeer server)
        {
            _handlers.Clear();
            gameObject.GetOrAddComponent<ClientPositionMonitor>().Setup(baseClient, server);

            AddSystem(new MovementClientSystem());
            foreach (var handlers in _handlers.Values)
            {
                handlers.Install(baseClient, server);
            }
        }

        private IEnumerator Polling()
        {
            while (_client.IsRunning)
            {
                _client.PollEvents();
                yield return new WaitForSeconds(1 / 15f);
            }
        }

        private void OnDestroy()
        {
            _client?.Stop();
        }
    }
}