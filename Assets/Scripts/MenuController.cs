using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField serverInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private TextMeshProUGUI errorMessage;
    private GameConnectionController _gameConnectionController;

    void Start()
    {
        connectButton.onClick.AddListener(ConnectClicked);
        _gameConnectionController = FindObjectOfType<GameConnectionController>();
    }

    private void ConnectClicked()
    {
        var address = "localhost";
        var port = 9050;
        var input = serverInput.text;
        if (!string.IsNullOrWhiteSpace(input))
        {
            var fullAddr = input.Split(":");
            if (fullAddr.Length != 2)
            {
                ErrorMessage("Write address as IP:port");
                return;
            }

            if (!int.TryParse(fullAddr[1], out port))
            {
                ErrorMessage("Port has to be a number");
                return;
            }
            address = fullAddr[0];
        }
        _gameConnectionController.SetConnectionDetails(address, port);
    }

    public void ErrorMessage(string message)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
    }
}
