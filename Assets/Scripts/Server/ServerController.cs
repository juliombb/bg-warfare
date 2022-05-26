using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Server
{
    public class ServerController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        public NetManager Server { get; private set; }
        private Dictionary<byte, IServerSystem> _handlers = new();

        private MovementServerSystem _movementServerSystem;

        private void Awake()
        {
            _handlers.Add((byte) ServerCommand.PositionOfPlayer, new MovementServerSystem(this));
        }

        private void Start()
        {
            _listener = new EventBasedNetListener();
            Server = new NetManager(_listener);
            Server.Start(9050 /* port */);

            _listener.ConnectionRequestEvent += request =>
            {
                if (Server.ConnectedPeersCount < 16 /* max connections */)
                    request.AcceptIfKey("BgWarfare");
                else
                    request.Reject();
            };

            _listener.PeerConnectedEvent += peer =>
            {
                Debug.Log($"We got connection: {peer.EndPoint}");
                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)ServerCommand.ClientPeerId);
                writer.Put(peer.Id);
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };
            
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var key = dataReader.GetByte();
                if (_handlers.ContainsKey(key))
                {
                    _handlers[key].Handle(fromPeer.Id, dataReader.GetRemainingBytes());
                }
                dataReader.Recycle();
                
            };

            Debug.Log("Started server");

            StartCoroutine(Polling());
        }

        private IEnumerator Polling()
        {
            while (Server.IsRunning)
            {
                Server.PollEvents();
                foreach (var system in _handlers.Values)
                {
                    system.Poll();
                }
                yield return new WaitForSeconds(1 / 15f);
            }
        }

        private void OnDestroy()
        {
            Server.Stop();
        }
    }
}
