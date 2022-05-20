using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _fire;
    [SerializeField] private AudioSource _walk;
    [SerializeField] private AudioSource _run;
    public void Fire()
    {
        _fire.Play();
    }

    public void Walk()
    {
        _run.Stop();
        if (!_walk.isPlaying)
        {
            _walk.Play();
        }
    }

    public void Run()
    {
        _walk.Stop();
        if (!_run.isPlaying)
        {
            _run.Play();
        }
    }

    public void Idle()
    {
        _walk.Stop();
        _run.Stop();
    }
}
