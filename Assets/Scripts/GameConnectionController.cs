using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameConnectionController : MonoBehaviour
{
    [SerializeField] private Canvas loadingScreen;
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
    }

    public void SetConnectionDetails(string addr, int port)
    {
        _addr = addr;
        _port = port;
        loadingScreen.enabled = true;
        SceneManager.LoadScene("ClientScene");
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        if (next.name == "ClientScene")
        {
            _clientControllerInstance = gameObject.GetOrAddComponent<ClientConnectionController>();
            var baseClient = GameObject.Find("BaseClient");
            _clientControllerInstance.SetLoadingScreen(loadingScreen);
            _clientControllerInstance.OnDisconnect(OnDisconnect);
            _clientControllerInstance.OnConnect(baseClient, _addr, _port);
        } else if (next.name == "Menu")
        {
            loadingScreen.enabled = false;
            GameObject.Find("MenuController").GetComponent<MenuController>()
                .ErrorMessage("You were disconnected from the server");
        }
    }

    private void OnDisconnect()
    {
        SceneManager.LoadScene("Menu");
    }
}
