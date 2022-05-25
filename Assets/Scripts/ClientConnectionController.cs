using System;
using System.Collections;
using System.Threading;
using LiteNetLib;
using UnityEngine;

namespace DefaultNamespace
{
    public class ClientConnectionController : MonoBehaviour
    {
        private EventBasedNetListener _listener;
        private NetManager _client;

        public void OnConnect()
        {
            if (_listener == null || _client == null)
            {
                _listener = new EventBasedNetListener();
                _client = new NetManager(_listener);
            }
            _client.Start();
            _client.Connect("localhost" /* host ip or name */, 9050 /* port */, "BgWarfare" /* text key or NetDataWriter */);
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Debug.Log($"We got: {dataReader.GetString(100 /* max length of string */)}");
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