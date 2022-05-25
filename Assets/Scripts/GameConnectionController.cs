using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class GameConnectionController : MonoBehaviour
{
    [SerializeField] private Button connectButton;
    private ClientConnectionController _clientConnectionController;

    public void SetConnectionController(ClientConnectionController clientConnectionController)
    {
        _clientConnectionController = clientConnectionController;
        connectButton.enabled = false;

    }
    void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
    }

    private void OnConnectClicked()
    {
        _clientConnectionController = gameObject.AddComponent<ClientConnectionController>();
        _clientConnectionController.OnConnect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
