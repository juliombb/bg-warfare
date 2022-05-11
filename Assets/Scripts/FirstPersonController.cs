using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float jumpForce = 70f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsule;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        transform.position += new Vector3(x, 0, z) * speed;

        if (Grounded())
        {
            float jump = Input.GetAxis("Jump");
            _rigidbody.AddForce(new Vector3(0, jump, 0) * jumpForce);
        }
    }

    bool Grounded()
    {
        return Physics.SphereCast(
            transform.position, _capsule.radius, Vector3.down, out _,
            (_capsule.height / 2f) - _capsule.radius + 0.1f);
    }
}
