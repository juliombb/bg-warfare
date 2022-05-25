using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Client;
using LiteNetLib;
using LiteNetLib.Utils;
using Server;
using UnityEngine;

namespace DefaultNamespace
{
    public class ClientConnectionController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        private NetManager _client;
        private Dictionary<byte, IClientSystem> _handlers = new();

        private void OnStart()
        {
            _handlers.Add((byte)ServerCommand.PositionOfPlayers, new MovementClientSystem());
        }
        public void OnConnect(GameObject baseClient)
        {
            foreach (var handlers    in _handlers.Values)
            {
                handlers   .Install(baseClient);
            }
            if (_listener == null || _client == null)
            {
                _listener = new EventBasedNetListener();
                _client = new NetManager(_listener);
            }
            _client.Start();
            _client.Connect("localhost" /* host ip or name */, 9050 /* port */, "BgWarfare" /* text key or NetDataWriter */);
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