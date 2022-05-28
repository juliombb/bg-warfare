using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Model;
using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    [SerializeField] private GameObject defaultModel;
    [SerializeField] private GameObject ragdoll;
    
    private Queue<PlayerSnapshot> _snapshotQueue = new();
    private Queue<PlayerSnapshot> _timedSnapshots = new();
    private PlayerSnapshot _previous;
    private PlayerSnapshot _next;
    private float _lastInterpolation = 0f;
    private Animator _animator;
    private static readonly int Walking = Animator.StringToHash("Walking");
    private int _animationCooldown = 0;
    private float? _timeToStart = null;
    private bool _checking = false;
    private int _id = -1;
    private static readonly int Fire = Animator.StringToHash("Fire");
    public int Id => _id;

    public void SetupId(int id)
    {
        if (_id != -1)
        {
            _id = id;
        }
    }

    private void StartCheck(float time)
    {
        _checking = true;
        PlayerSnapshot lastSnapshot = null;
        PlayerSnapshot nextSnapshot = null; 
        foreach (var playerSnapshot in _timedSnapshots)
        {
            if (playerSnapshot.Time < time)
            {
                lastSnapshot = playerSnapshot;
            }
            else
            {
                nextSnapshot = playerSnapshot;
                break;
            }
        }

        if (nextSnapshot == null && _previous != null)
        {
            nextSnapshot = _previous;
        }

        if (lastSnapshot == null || nextSnapshot == null)
        {
            transform.position = _previous?.Position ?? transform.position;
            return;
        }
        
        float duration = (lastSnapshot.Sequence - nextSnapshot.Sequence) * Config.TickRate;
        float elapsedTime = time - lastSnapshot.Time;

        var smoothStep = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(elapsedTime / duration));
        transform.position = Vector3.Lerp(_previous.Position, _next.Position, smoothStep);
    }

    private void FinishCheck()
    {
        _checking = false;
    }

    public void OnShot()
    {
        _animator.SetTrigger(Fire);
    }

    private void Start()
    {
        _animator = defaultModel.GetComponent<Animator>();
    }

    public void OnNewSnapshot(PlayerSnapshot snapshot)
    {
        if (_snapshotQueue.Count > 32 || (_snapshotQueue.Count > 0 && _snapshotQueue.Peek().Sequence >= snapshot.Sequence))
        {
            return;
        }
        
        _snapshotQueue.Enqueue(snapshot);
    }

    private void FixedUpdate()
    {
        if (_snapshotQueue.Count <= 0)
        {
            if (
                _previous != null
                && Time.time - _lastInterpolation >= Config.TickRate * (_next.Sequence - _previous.Sequence)
            )
            {
                _previous = null;
                _timeToStart = null;
            }

            return;
        }
        
        float time = Time.time;

        if (_previous == null)
        {
            if (_timeToStart == null)
            {
                _timeToStart = time + Config.RemoteLagSecs;
                return;
            }
            if (_timeToStart > time) 
            {
                return;
            }
            var currentSnapshot = _snapshotQueue.Dequeue();
            _previous = _next = currentSnapshot;
            // Debug.Log("Started interpolation");
            return;
        }

        if (time - _lastInterpolation >= Config.TickRate * (_next.Sequence - _previous.Sequence))
        {
            _previous = _next;
            _next = _snapshotQueue.Dequeue();
            _lastInterpolation = time;
            if (_timedSnapshots.Count > 5)
            {
                _timedSnapshots.Dequeue();
            }
            _timedSnapshots.Enqueue(_previous.Timed(time));
            // Debug.Log($"New snapshot: {_previous} {_next}");
        }
    }

    private void Update()
    {
        if (_checking)
        {
            return;
        }
        if (_previous == null)
        {
            if (_animationCooldown++ > 5)
            {
                _animator.SetBool(Walking, false);
            }
            return;
        }

        var time = Time.time;
        float duration = (_next.Sequence - _previous.Sequence) * Config.TickRate;
        float elapsedTime = time - _lastInterpolation;
        if (elapsedTime > duration)
        {
            return;
        }

        var smoothStep = Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / duration);
        transform.position = Vector3.Lerp(_previous.Position, _next.Position, smoothStep);
        transform.rotation = Quaternion.Slerp(
            _previous.Rotation, 
            _next.Rotation,
            elapsedTime / duration
        );

        _animator.SetBool(Walking, true);

        _animationCooldown = 0;
    }

    public bool Moving()
    {
        return _snapshotQueue.Count > 0;
    }
    
    public void Die(Vector3 hitPosition, Vector3 hitDirection)
    {
        if (defaultModel == null)
        {
            return;
        }

        this.GetComponent<CapsuleCollider>().enabled = false;
        var rd = Instantiate(ragdoll, defaultModel.transform.position, defaultModel.transform.rotation);
        var rigidbodies = rd.GetComponentsInChildren<Rigidbody>();
        var closest = rigidbodies.Length > 0 ? rigidbodies[0] : null;
        foreach (var rb in rigidbodies)
        {
            if (Vector3.Distance(closest.transform.position, hitPosition) >
                Vector3.Distance(rb.transform.position, hitPosition))
            {
                closest = rb;
            }
        }
        
        Destroy(defaultModel);

        if (closest != null)
        {
            closest.AddForce(hitDirection * 100, ForceMode.VelocityChange);
        }

        StartCoroutine(DestroyAfter3s());
    }

    private IEnumerator DestroyAfter3s()
    {
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }
}
