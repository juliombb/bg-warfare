using System;
using System.Collections;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Server
{
    public class ServerController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        private NetManager _server;
        private void Start()
        {
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener);
            _server.Start(9050 /* port */);

            _listener.ConnectionRequestEvent += request =>
            {
                if(_server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("BgWarfare");
                else
                    request.Reject();
            };

            _listener.PeerConnectedEvent += peer =>
            {
                Debug.Log($"We got connection: {peer.EndPoint}"); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };
            
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Debug.Log($"We got: {dataReader.GetString(100 /* max length of string */)}");
                dataReader.Recycle();
            };
            
            Debug.Log("Started server");

            StartCoroutine(Polling());
        }

        private IEnumerator Polling()
        {
            while (_server.IsRunning)
            {
                _server.PollEvents();
                yield return new WaitForSeconds(1 / 15f);
            }
        }

        private void OnDestroy()
        {
            _server.Stop();
        }
    }
}
