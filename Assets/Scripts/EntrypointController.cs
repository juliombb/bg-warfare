using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrypointController : MonoBehaviour
{
    void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (arg == "-server")
            {
                SceneManager.LoadScene("ServerScene");
            }
        }

        SceneManager.LoadScene("Menu");
    }
}
