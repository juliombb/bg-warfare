using System.Collections.Generic;
using DefaultNamespace;
using Model;
using UnityEngine;

public class RemotePlayersController : MonoBehaviour
{
    [SerializeField] private GameObject _remotePlayerPrefab;
    
    private readonly Dictionary<int, RemotePlayerController> _connectedPlayers = new();

    public RemotePlayerController GetRemotePlayer(int playerId)
    {
        if (_connectedPlayers.ContainsKey(playerId))
        {
            return _connectedPlayers[playerId];
        }

        return null;
    }

    public void OnPlayerEnter(int userId)
    {
        if (_connectedPlayers.ContainsKey(userId)) return;
        var remotePlayer = Instantiate(_remotePlayerPrefab);
        var remoteController = remotePlayer.GetComponent<RemotePlayerController>();
        remoteController.SetupId(userId);
        _connectedPlayers[userId] = remoteController;
    }

    public void OnPlayerLeft(int userId)
    {
        if (!_connectedPlayers.ContainsKey(userId)) return;

        var remotePlayer = _connectedPlayers[userId];
        _connectedPlayers.Remove(userId);
        Destroy(remotePlayer.gameObject);
    }

    public void OnPositionUpdate(int player, PlayerSnapshot snapshot)
    {
        if (!_connectedPlayers.ContainsKey(player))
        {
            OnPlayerEnter(player);
        }
        _connectedPlayers[player].OnNewSnapshot(snapshot);
    }
    
    public void OnShot(int player)
    {
        if (!_connectedPlayers.ContainsKey(player))
        {
            return;
        }
        _connectedPlayers[player].OnShot();
    }
    
    public void TakeShot(int player, Vector3 position, Vector3 direction)
    {
        if (!_connectedPlayers.ContainsKey(player))
        {
            return;
        }
        _connectedPlayers[player].Die(position, direction); // todo take shot
    }
}