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
    private Rigidbody _hoveredRb;

    private float _dragDistance;
    private float _oldDrag;
    private float _oldAngularDrag;

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

    private void Update()
    {
        UpdateHover();
    }

    private void FixedUpdate()
    {
        if (_draggedRb != null) DragObject();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_draggedRb != null) return;

        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(GetPointerPosition()), out RaycastHit hit)) return;

        Dragble dragble = hit.transform.GetComponentInParent<Dragble>();
        if (dragble == null) return;

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

    private void UpdateHover()
    {
        if (_draggedRb != null) return;

        Ray ray = _mainCamera.ScreenPointToRay(GetPointerPosition());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var dragble = hit.transform.GetComponentInParent<Dragble>();

            if (dragble != null)
            {
                Rigidbody rb = dragble.GetComponent<Rigidbody>();

                if (_hoveredRb != rb)
                {
                    ClearHover();
                    _hoveredRb = rb;
                    ApplyHighlight(_hoveredRb, true, Color.green);
                }
                return;
            }
        }

        if (!_mouseClick.action.IsPressed())
            ClearHover();
    }

    private void ClearHover()
    {
        if (_hoveredRb == null) return;

        ApplyHighlight(_hoveredRb, false, Color.green);
        _hoveredRb = null;
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

        ApplyHighlight(rb, true, Color.blue);
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

        ApplyHighlight(_draggedRb, false, Color.blue);
        _draggedRb = null;
    }

    private void ApplyHighlight(Rigidbody rb, bool active, Color color)
    {
        foreach (var r in rb.GetComponentsInChildren<Renderer>())
        {
            if (!r) continue;

            r.GetPropertyBlock(_block);

            if (active)
            {
                Color changingColor = color;

                if (r.sharedMaterial.HasProperty("_BaseColor")) _block.SetColor("_BaseColor", changingColor);
                else if (r.sharedMaterial.HasProperty("_Color")) _block.SetColor("_Color", changingColor);
            }
            else
            {
                _block.Clear();
            }

            r.SetPropertyBlock(_block);
        }
    }
}