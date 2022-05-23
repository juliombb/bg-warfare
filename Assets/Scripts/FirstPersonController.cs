using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform raycaster;
    [SerializeField] private GameObject[] legs;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float ySense = 2f;
    [SerializeField] private float xSense = 1.5f;
    [SerializeField] private float jumpForce = 70f;
    [SerializeField] private GameObject cube;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsule;
    private bool _cursorLocked = false;
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Fire = Animator.StringToHash("Fire");

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        HandleShot();
    }

    void FixedUpdate()
    {
        HandleAim();

        var actualSpeed = CheckSprint();

        HandleMovement(actualSpeed);

        HandleJump();

        UpdateCursor();
    }

    private void HandleShot()
    {
        if (Input.GetMouseButton(0))
        {
            animator.SetTrigger(Fire);
            var raycasterTransform = raycaster.transform;
            Physics.Raycast(raycasterTransform.position, raycasterTransform.forward, out var hit, 200);
            if (hit.collider == null)
            {
                return;
            }

            var target = hit.collider.gameObject;
            if (target.CompareTag("RemotePlayer"))
            {
                target.GetComponent<RemotePlayerController>().Die(hit.point, raycasterTransform.forward);
            }
        }
    }

    private void HandleAim()
    {
        float yRot = Input.GetAxis("Mouse X") * ySense;
        float xRot = Input.GetAxis("Mouse Y") * xSense;

        transform.localRotation *= Quaternion.Euler(0, yRot, 0);
        var leg1Rot = legs[0].transform.rotation;
        var leg2Rot = legs[1].transform.rotation;

        camera.transform.localRotation = ClampRotation(
            camera.transform.localRotation * Quaternion.Euler(-xRot, 0, 0),
            new Vector3(90f, 360f)
        );

        legs[0].transform.rotation = leg1Rot;
        legs[1].transform.rotation = leg2Rot;
    }

    private float CheckSprint()
    {
        var actualSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            actualSpeed = speed * 1.5f;
            animator.SetBool(Running, true);
        }
        else
        {
            animator.SetBool(Running, false);
        }

        return actualSpeed;
    }

    private void HandleJump()
    {
        if (Grounded())
        {
            float jump = Input.GetAxis("Jump");
            _rigidbody.AddForce(new Vector3(0, jump, 0) * jumpForce);
        }
    }

    private void HandleMovement(float actualSpeed)
    {
        float x = Input.GetAxis("Horizontal") * actualSpeed;
        float z = Input.GetAxis("Vertical") * actualSpeed;

        var position = transform.position;
        var previousPosition = position;
        position += camera.transform.forward * z + camera.transform.right * x;
        position.y = previousPosition.y;
        transform.position = position;

        animator.SetBool(Walking, Math.Abs(x) > 0.0001f || Math.Abs(z) > 0.0001f);
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
