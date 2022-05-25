using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class GameConnectionController : MonoBehaviour
{
    private ClientConnectionController _clientConnectionController;
    [SerializeField] private GameObject baseClient;

    public void SetConnectionController(ClientConnectionController clientConnectionController)
    {
        _clientConnectionController = clientConnectionController;
    }

    private void OnConnectClicked()
    {
        _clientConnectionController = gameObject.AddComponent<ClientConnectionController>();
        _clientConnectionController.OnConnect(baseClient);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            OnConnectClicked();
        }
    }
}
