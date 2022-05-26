using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    [SerializeField] private GameObject defaultModel;
    [SerializeField] private GameObject ragdoll;
    
    private Queue<PlayerSnapshot> _snapshotQueue = new Queue<PlayerSnapshot>();
    private PlayerSnapshot _previous;
    private PlayerSnapshot _next;
    private float _lastInterpolation;
    private Animator _animator;
    private static readonly int Walking = Animator.StringToHash("Walking");
    private int _animationCooldown = 0;

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

    private void Update()
    {
        if (_snapshotQueue.Count <= 0)
        {
            if (_animationCooldown++ > 15)
            {
                _animator.SetBool(Walking, false);
            }
            return;
        }

        var currentSnapshot = _snapshotQueue.Peek();
        if (_previous == null)
        {
            transform.position = currentSnapshot.Position;
            transform.rotation = currentSnapshot.Rotation;
            _next = currentSnapshot;
            _snapshotQueue.Dequeue();
            return;
        }

        float time = Time.time;
        if (time - _lastInterpolation >= Config.TickRate * (_next.Sequence - _previous.Sequence))
        {
            _previous = _next;
            _next = currentSnapshot;
            _snapshotQueue.Dequeue();
        }

        float duration = (_previous.Sequence - _next.Sequence) * Config.TickRate;
        float elapsedTime = time - _lastInterpolation;
        transform.position = Vector3.Lerp(_previous.Position, _next.Position, elapsedTime / duration);
        transform.rotation = Quaternion.Slerp(
            _previous.Rotation, 
            _next.Rotation,
            elapsedTime / duration
        );

        _lastInterpolation = time;
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
