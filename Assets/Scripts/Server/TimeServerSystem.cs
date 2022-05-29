using System;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

namespace Server
{
    public class TimeServerSystem: IServerSystem
    {
        public ServerCommand CommandKey => ServerCommand.InitialTime;
        private readonly RemotePlayersController _remotePlayersController;
        private Dictionary<int, float> _initialTimePerUser = new();
        private Dictionary<int, float> _initialServerTimePerUser = new();

        public float UserTimeToServerTime(int userId, float userTime)
        {
            if (_initialTimePerUser.ContainsKey(userId) && _initialServerTimePerUser.ContainsKey(userId))
            {
                var userStartTime = _initialTimePerUser[userId];
                var serverStartTime = _initialServerTimePerUser[userId];
                var offset = userTime - userStartTime;
                return serverStartTime + offset;
            }

            Debug.Log("Tried to get time of user who did not register initial time");
            return Time.time;
        }
        public void Handle(int peer, byte[] data)
        {
            _initialTimePerUser[peer] = BitConverter.ToSingle(data);
            _initialServerTimePerUser[peer] = Time.time;
        }

        public void OnPeerEnter(NetPeer peer) {}

        public void Poll() { }
    }
}