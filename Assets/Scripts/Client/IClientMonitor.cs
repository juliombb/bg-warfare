using LiteNetLib;
using UnityEngine;

namespace Client
{
    public interface IClientMonitor
    {
        public void Setup(GameObject baseClient, ServerData server);
    }
}