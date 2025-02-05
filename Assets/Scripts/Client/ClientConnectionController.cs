using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Client;
using LiteNetLib;
using LiteNetLib.Utils;
using Server;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class ClientConnectionController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        private NetManager _client;
        private Dictionary<byte, IClientSystem> _handlers = new();
        private NetPeer _server;
        private int _clientPeerId = -1;
        private Canvas _loadingScreen;
        private List<MonoBehaviour> _monitors;

        private Action _onDisconnect;
        private TMP_Text _rtt;

        public void OnDisconnect(Action action)
        {
            _onDisconnect = action;
        }
        private void AddSystem(IClientSystem system)
        {
            _handlers.Add((byte)system.CommandKey, system);
        }

        public void OnConnect(GameObject baseClient, string address, int port)
        {
            if (_listener == null || _client == null)
            {
                _listener = new EventBasedNetListener();
                _client = new NetManager(_listener);
            }
            _client.Start();
            _client.Connect(address /* host ip or name */, port /* port */, "BgWarfare" /* text key or NetDataWriter */);
            _listener.PeerConnectedEvent += netPeer =>
            {
                _server = netPeer;
                _rtt = GameObject.Find("RTTText").GetComponent<TMP_Text>();
            };
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                if (_server.Id != fromPeer.Id) return;
                var command = dataReader.GetByte();
                if (command == (byte)ServerCommand.ClientPeerId && _clientPeerId == -1)
                {
                    _clientPeerId = dataReader.GetInt();
                    var server = new ServerData(_server, _clientPeerId);
                    Debug.Log($"Connected! I am peer {_clientPeerId}");
                    SetupServer(baseClient, server);
                    _loadingScreen.enabled = false;
                }

                if (_clientPeerId == -1) return;
                if (_handlers.ContainsKey(command))
                {
                    _handlers[command].Handle(dataReader.GetRemainingBytes());
                }
                dataReader.Recycle();
            };

            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                Debug.Log($"peer disconnected {peer?.Id} my: {_clientPeerId} server: {_server?.Id} reason: {info.Reason}");
                if (info.Reason == DisconnectReason.ConnectionFailed || peer?.Id == 0)
                {
                    _clientPeerId = -1;
                    Destroy(gameObject.GetOrAddComponent<ClientPositionMonitor>());
                    Destroy(gameObject.GetOrAddComponent<ClientShotMonitor>());
                    _onDisconnect?.Invoke();
                    _onDisconnect = null;
                }
            };

            StartCoroutine(Polling());
        }

        private void SetupServer(GameObject baseClient, ServerData server)
        {
            _handlers.Clear();
            gameObject.GetOrAddComponent<ClientPositionMonitor>().Setup(baseClient, server);
            gameObject.GetOrAddComponent<ClientShotMonitor>().Setup(baseClient, server);

            AddSystem(new MovementClientSystem());
            AddSystem(new PeerDisconnectClientSystem());
            AddSystem(new ShotClientSystem());
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
                if (_rtt != null && _server != null)
                {
                    _rtt.text = $"Ping: {_server.Ping}ms";
                }

                yield return new WaitForSeconds(Config.TickRate);
            }
        }

        private void OnDestroy()
        {
            _client?.Stop();
        }

        public void SetLoadingScreen(Canvas loadingScreen)
        {
            _loadingScreen = loadingScreen;
        }
    }
}