using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private GameObject cameraGo;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform raycaster;
    [SerializeField] private GameObject cube;
    private float speed = 0.25f;
    private float ySense = 2f;
    private float xSense = 1.5f;
    private float jumpForce = 200f;
    private float fireRate = 0.134f;

    private Camera _camera;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsule;
    private float _lastShot = 0;
    private bool _cursorLocked = false;
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Fire = Animator.StringToHash("Fire");

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
        _camera = cameraGo.GetComponent<Camera>();
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

    private void OnShotEntered() {
        var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(mouseRay.origin, mouseRay.direction, out var hit, 200);
            
        if (hit.collider == null)
        {
            return;
        }

        //Instantiate(cube, hit.point, Quaternion.identity);

        var target = hit.collider.gameObject;
        if (target.CompareTag("RemotePlayer"))
        {
            target.GetComponent<RemotePlayerController>().Die(hit.point, mouseRay.direction);
        }
    }

    private void HandleShot()
    {
        if (Input.GetMouseButton(0))
        {
            animator.SetTrigger(Fire);
            if (Time.time - _lastShot > fireRate)
            {
                _lastShot = Time.time;
                OnShotEntered();
            }
        }
    }

    private void HandleAim()
    {
        float yRot = Input.GetAxis("Mouse X") * ySense;
        float xRot = Input.GetAxis("Mouse Y") * xSense;

        transform.localRotation *= Quaternion.Euler(0, yRot, 0);

        cameraGo.transform.localRotation = ClampRotation(
            cameraGo.transform.localRotation * Quaternion.Euler(-xRot, 0, 0),
            new Vector3(45f, 360f)
        );
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
        position += cameraGo.transform.forward * z + cameraGo.transform.right * x;
        position.y = previousPosition.y;
        transform.position = position;

        animator.SetBool(Walking, Math.Abs(x) > 0.0001f || Math.Abs(z) > 0.0001f);
    }

    bool Grounded()
    {
        return Physics.SphereCast(
            transform.position + _capsule.center, _capsule.radius, Vector3.down, out _,
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
