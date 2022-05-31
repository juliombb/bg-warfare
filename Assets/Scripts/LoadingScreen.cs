using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("LoadingScreen");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
        gameObject.SetActive(false);
    }
}
