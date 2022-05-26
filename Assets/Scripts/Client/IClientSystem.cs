using LiteNetLib;
using Server;
using UnityEngine;

namespace Client
{
    public interface IClientSystem
    {
        public ServerCommand CommandKey { get; }

        void Handle(byte[] data);
        void Install(GameObject baseClient, ServerData server);
    }
}