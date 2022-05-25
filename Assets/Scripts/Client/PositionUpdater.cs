using System;
using DefaultNamespace;
using UnityEngine;

public class PositionUpdater : MonoBehaviour
{
    private float _lastUpdate = 0f;
    private void Update()
    {
        if (Time.time - _lastUpdate > Config.TickRate)
        {
            // todo send update
        }
    }
}