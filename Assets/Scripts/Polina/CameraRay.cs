using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraRay : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _mouseClick;

    [Header("Drag Settings")]
    [SerializeField] private float _dragForce = 100f;
    [SerializeField] private float _velocityDamper = 10f;
    [SerializeField] private float _drag = 15f;
    [SerializeField] private float _angularDrag = 5f;

    [Header("Limb Constraint")]
    [SerializeField] private Rigidbody _mainBody;
    [SerializeField] private float _maxLimbDistance = 1.5f;

    public static event Action<GameObject> OnObjectReleased;

    private Camera _mainCamera;
    private Rigidbody _draggedRb;
    private float _dragDistance;
    private float _oldDrag;
    private float _oldAngularDrag;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _mouseClick.action.Enable();
        _mouseClick.action.started += OnClickStarted;
        _mouseClick.action.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        _mouseClick.action.started -= OnClickStarted;
        _mouseClick.action.canceled -= OnClickCanceled;
        _mouseClick.action.Disable();

        ReleaseObject();
    }

    private void FixedUpdate()
    {
        if (_draggedRb != null)
            DragObject();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_draggedRb != null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(GetPointerPosition());

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        Dragble dragble = hit.transform.GetComponentInParent<Dragble>();

        if (dragble == null)
            return;

        StartDrag(hit, dragble.GetComponent<Rigidbody>());
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        ReleaseObject();
    }

    private Vector2 GetPointerPosition()
    {
        return Pointer.current?.position.ReadValue() ?? Vector2.zero;
    }

    private void StartDrag(RaycastHit hit, Rigidbody rb)
    {
        if (rb == null)
            return;

        _draggedRb = rb;

        _dragDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);

        _oldDrag = _draggedRb.linearDamping;
        _oldAngularDrag = _draggedRb.angularDamping;

        _draggedRb.linearDamping = _drag;
        _draggedRb.angularDamping = _angularDrag;
    }

    private void DragObject()
    {
        Vector2 pointerPos = GetPointerPosition();

        Vector3 targetPos = _mainCamera.ScreenToWorldPoint(
            new Vector3(pointerPos.x, pointerPos.y, _dragDistance));

        Vector3 fromBody = targetPos - _mainBody.position;

        if (fromBody.magnitude > _maxLimbDistance)
            targetPos = _mainBody.position + fromBody.normalized * _maxLimbDistance;

        Vector3 delta = targetPos - _draggedRb.position;
        Vector3 force = delta * _dragForce - _draggedRb.linearVelocity * _velocityDamper;

        _draggedRb.AddForce(force);
    }

    private void ReleaseObject()
    {
        if (_draggedRb == null)
            return;

        OnObjectReleased?.Invoke(_draggedRb.gameObject);

        _draggedRb.linearDamping = _oldDrag;
        _draggedRb.angularDamping = _oldAngularDrag;

        _draggedRb = null;
    }
}