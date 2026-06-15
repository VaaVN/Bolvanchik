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

    [Header("Constraint")]
    [SerializeField] private Rigidbody _mainBody;
    [SerializeField] private float _maxLimbDistance = 1.5f;

    public static event Action<GameObject> OnObjectReleased;

    private Camera _mainCamera;
    private Rigidbody _draggedRb;
    private float _dragDistance;
    private float _oldDrag;
    private float _oldAngularDrag;

    private Renderer[] _dragRenderers;
    private MaterialPropertyBlock _block;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _block = new MaterialPropertyBlock();
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
        if (_draggedRb != null) DragObject();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_draggedRb != null) return;

        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(GetPointerPosition()), out RaycastHit hit)) return;

        var dragble = hit.transform.GetComponentInParent<Dragble>();
        if (!dragble) return;

        StartDrag(hit, dragble.GetComponent<Rigidbody>());
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        ReleaseObject();
    }

    private Vector2 GetPointerPosition()
    {
        return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
    }

    private void StartDrag(RaycastHit hit, Rigidbody rb)
    {
        if (!rb) return;

        _draggedRb = rb;
        _dragDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);

        _oldDrag = rb.linearDamping;
        _oldAngularDrag = rb.angularDamping;

        rb.linearDamping = _drag;
        rb.angularDamping = _angularDrag;

        ApplyHighlight(rb, true);
    }

    private void DragObject()
    {
        Vector2 pointerPos = GetPointerPosition();

        Vector3 targetPos = _mainCamera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, _dragDistance));

        Vector3 offset = targetPos - _mainBody.position;

        if (offset.magnitude > _maxLimbDistance) targetPos = _mainBody.position + offset.normalized * _maxLimbDistance;

        Vector3 delta = targetPos - _draggedRb.position;

        Vector3 force = delta * _dragForce - _draggedRb.linearVelocity * _velocityDamper;

        _draggedRb.AddForce(force);
    }

    private void ReleaseObject()
    {
        if (!_draggedRb) return;

        OnObjectReleased?.Invoke(_draggedRb.gameObject);

        _draggedRb.linearDamping = _oldDrag;
        _draggedRb.angularDamping = _oldAngularDrag;

        ApplyHighlight(_draggedRb, false);

        _draggedRb = null;
    }

    private void ApplyHighlight(Rigidbody rb, bool active)
    {
        _dragRenderers = rb.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < _dragRenderers.Length; i++)
        {
            var r = _dragRenderers[i];
            if (!r) continue;

            r.GetPropertyBlock(_block);

            if (active)
            {
                Color blue = Color.blue;

                if (r.sharedMaterial.HasProperty("_BaseColor"))
                    _block.SetColor("_BaseColor", blue);
                else if (r.sharedMaterial.HasProperty("_Color"))
                    _block.SetColor("_Color", blue);
            }
            else
            {
                _block.Clear();
            }

            r.SetPropertyBlock(_block);
        }
    }
}