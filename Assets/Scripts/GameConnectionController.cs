using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameConnectionController : MonoBehaviour
{
    private ClientConnectionController _clientConnectionController;
    [SerializeField] private GameObject loadingScreen;

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
    }

    public void SetConnectionDetails(string addr, int port)
    {
        _addr = addr;
        _port = port;
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("ClientScene");
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        if (next.name == "ClientScene")
        {
            _clientConnectionController = gameObject.AddComponent<ClientConnectionController>();
            var baseClient = GameObject.Find("BaseClient");
            _clientConnectionController.SetLoadingScreen(loadingScreen);
            _clientConnectionController.OnDisconnect(OnDisconnect);
            _clientConnectionController.OnConnect(baseClient, _addr, _port);
        } else if (next.name == "Menu")
        {
            Destroy(_clientConnectionController);
            loadingScreen.SetActive(false);
            GameObject.Find("MenuController").GetComponent<MenuController>()
                .ErrorMessage("You were disconnected from the server");
        }
    }

    private void OnDisconnect()
    {
        SceneManager.LoadScene("Menu");
    }
}
