using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private GameObject cameraGo;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform raycaster;
    [SerializeField] private GameObject capsule;
    [SerializeField] private Material altCapsuleMaterial;
    private event Action<Vector3, Vector3, int> shotListener;
    private float speed = 0.25f;
    private float ySense = 2f;
    private float xSense = 1.5f;
    private float jumpForce = 200f;
    private float fireRate = 0.134f;
    private GameObject _capsule1;
    private GameObject _capsule2;

    private Camera _camera;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsule;
    private float _lastShot = 0;
    private bool _cursorLocked = false;
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Fire = Animator.StringToHash("Fire");
    private bool _randomWalk = false;
    private float _startOfRandomWalk = 0.0f;

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

    private void HandleRandomWalk()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _randomWalk = !_randomWalk;
            if (_randomWalk)
            {
                _startOfRandomWalk = Time.time;
            }
        }
    }

    public void OnShot(Action<Vector3, Vector3, int> listener)
    {
        shotListener += listener;
    }
    
    public void RemoveOnShot(Action<Vector3, Vector3, int> listener)
    {
        shotListener -= listener;
    }

    public void TakeShot()
    {
        Debug.Log("I was shot!");
    }

    void FixedUpdate()
    {
        HandleAim();

        var actualSpeed = CheckSprint();

        HandleRandomWalk();

        HandleMovement(actualSpeed);

        HandleJump();

        UpdateCursor();
    }

    public void RenderCapsule(Vector3 position, bool alt)
    {
        if (Vector3.Distance(position, _camera.ScreenPointToRay(Input.mousePosition).origin) < 1f)
        {
            return;
        } 
        Debug.Log($"Rendering {alt} capsule at {position}");

        if (alt)
        {
            RenderCapsule(position, ref _capsule1, true);
        }
        else
        {
            RenderCapsule(position, ref _capsule2, false);
        }
    }

    private void RenderCapsule(Vector3 position, ref GameObject obj, bool alt)
    {
        if (obj == null)
        {
            obj = Instantiate(capsule, position, Quaternion.identity);
        }
        else
        {
            obj.transform.position = position;
        }

        if (alt)
        {
            var objRenderer = obj.GetComponentInChildren<MeshRenderer>();
            objRenderer.material = altCapsuleMaterial;
        }
    }

    private void OnShotEntered() {
        var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(mouseRay.origin, mouseRay.direction, out var hit, Config.MaxShotDistance);

        if (hit.collider == null)
        {
            shotListener?.Invoke(mouseRay.origin, mouseRay.direction, -1);
            return;
        }

        //RenderCapsule(hit.point)

        var target = hit.collider.gameObject;
        if (target.CompareTag("RemotePlayer"))
        {
            var remotePlayer = target.GetComponent<RemotePlayerController>();
            RenderCapsule(hit.collider.transform.position, true);
            shotListener?.Invoke(mouseRay.origin, mouseRay.direction, remotePlayer.Id);
        }
        else
        {
            shotListener?.Invoke(mouseRay.origin, mouseRay.direction, -1);
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
        if (_randomWalk)
        {
            var time = Time.time - _startOfRandomWalk;
            x = Mathf.Sin(time) * actualSpeed;
        }

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
