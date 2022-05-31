using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameConnectionController : MonoBehaviour
{
    private Canvas _loadingScreen;
    private ClientConnectionController _clientControllerInstance;

    private string _addr;
    private int _port;
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameControl");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += ChangedActiveScene;
        _loadingScreen = GetComponent<Canvas>();
        _loadingScreen.enabled = false;
    }

    public void SetConnectionDetails(string addr, int port)
    {
        _addr = addr;
        _port = port;
        _loadingScreen.enabled = true;
        SceneManager.LoadScene("ClientScene");
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        if (gameObject == null) return;
        if (next.name == "ClientScene")
        {
            var clientControllerInstance = gameObject.GetOrAddComponent<ClientConnectionController>();
            var baseClient = GameObject.Find("BaseClient");
            clientControllerInstance.SetLoadingScreen(_loadingScreen);
            clientControllerInstance.OnDisconnect(OnDisconnect);
            clientControllerInstance.OnConnect(baseClient, _addr, _port);
        } else if (next.name == "Menu")
        {
            Destroy(gameObject.GetOrAddComponent<ClientConnectionController>());
            _loadingScreen.enabled = false;
            GameObject.Find("MenuController").GetComponent<MenuController>()
                .ErrorMessage("You were disconnected from the server");
        }
    }

    private void OnDisconnect()
    {
        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= ChangedActiveScene;
    }
}
