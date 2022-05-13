using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float ySense = 2f;
    [SerializeField] private float xSense = 1.5f;
    [SerializeField] private float jumpForce = 70f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsule;
    private bool _cursorLocked = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X") * ySense;
        float xRot = Input.GetAxis("Mouse Y") * xSense;
        
        transform.localRotation *= Quaternion.Euler(0, yRot, 0);

        camera.transform.localRotation = ClampRotation(
            camera.transform.localRotation * Quaternion.Euler(-xRot, 0, 0),
            new Vector3(90f, 360f)
        );

        float x = Input.GetAxis("Horizontal") * speed;
        float z = Input.GetAxis("Vertical") * speed;

        var position = transform.position;
        var previousPosition = position;
        position += camera.transform.forward * z + camera.transform.right * x;
        position.y = previousPosition.y;
        transform.position = position;

        if (Grounded())
        {
            float jump = Input.GetAxis("Jump");
            _rigidbody.AddForce(new Vector3(0, jump, 0) * jumpForce);
        }

        UpdateCursor();
    }

    bool Grounded()
    {
        return Physics.SphereCast(
            transform.position, _capsule.radius, Vector3.down, out _,
            (_capsule.height / 2f) - _capsule.radius + 0.1f);
    }

    public void UpdateCursor()
    {
        if (!_cursorLocked)
        {
            if (!Input.GetMouseButtonUp(0)) return;
            _cursorLocked = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }

        if (!Input.GetKeyUp(KeyCode.Escape)) return;
        _cursorLocked = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public static Quaternion ClampRotation(Quaternion q, Vector3 bounds)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
 
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
 
        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
 
        float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
        angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
        q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);
 
        return q;
    }
}
