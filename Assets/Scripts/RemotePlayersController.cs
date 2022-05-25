using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class RemotePlayersController : MonoBehaviour
{
    [SerializeField] private GameObject _remotePlayerPrefab;
    
    private readonly Dictionary<int, RemotePlayerController> _connectedPlayers = new();

    public void OnPlayerEnter(int userId)
    {
        if (_connectedPlayers.ContainsKey(userId)) return;
        var remotePlayer = Instantiate(_remotePlayerPrefab);
        var remoteController = remotePlayer.GetComponent<RemotePlayerController>();
        _connectedPlayers[userId] = remoteController;
    }

    public void OnPlayerLeft(int userId)
    {
        if (!_connectedPlayers.ContainsKey(userId)) return;

        var playerObject = _connectedPlayers[userId];
        _connectedPlayers.Remove(userId);
        Destroy(playerObject);
    }

    public void OnPositionUpdate(int player, PlayerSnapshot snapshot)
    {
        if (!_connectedPlayers.ContainsKey(player))
        {
            OnPlayerEnter(player);
        }
        _connectedPlayers[player].OnNewSnapshot(snapshot);
    }
}