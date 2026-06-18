using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRay : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _mouseClick;

    [Header("Constraint")]
    [SerializeField] private Rigidbody _mainBody;
    [SerializeField] private float _maxLimbDistance = 1.5f;

    private Camera _mainCamera;
    private float _dragDistance;

    private Dragble _draggedObject;
    private Dragble _hoveredObject;

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

    private void Update()
    {
        UpdateHover();
    }

    private void FixedUpdate()
    {
        if (_draggedObject != null) DragObject();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_draggedObject != null) return;

        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(GetPointerPosition()), out RaycastHit hit)) return;

        Dragble dragble = hit.transform.GetComponentInParent<Dragble>();
        if (dragble == null) return;

        StartDrag(hit, dragble);
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
        if (_draggedObject != null) return;

        Ray ray = _mainCamera.ScreenPointToRay(GetPointerPosition());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var dragble = hit.transform.GetComponentInParent<Dragble>();

            if (dragble != null)
            {
                if (_hoveredObject != dragble)
                {
                    ClearHover();
                    _hoveredObject = dragble;
                    _hoveredObject.SetHover(true); 
                }
                return;
            }
        }

        if (!_mouseClick.action.IsPressed())
            ClearHover();
    }

    private void ClearHover()
    {
        if (_hoveredObject == null) return;
        _hoveredObject.SetHover(false); 
        _hoveredObject = null;
    }

    private void StartDrag(RaycastHit hit, Dragble dragble)
    {
        _draggedObject = dragble;
        _dragDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);
        _draggedObject.OnGrab(_dragDistance);
    }

    private void ReleaseObject()
    {
        if (_draggedObject == null) return;
        _draggedObject.OnRelease();
        _draggedObject = null;
    }

    private void DragObject()
    {
        Vector2 pointerPos = GetPointerPosition();
        Vector3 targetPos = _mainCamera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, _dragDistance));

        Vector3 offset = targetPos - _mainBody.position;
        if (offset.magnitude > _maxLimbDistance)
            targetPos = _mainBody.position + offset.normalized * _maxLimbDistance;

        _draggedObject.ApplyDragForce(targetPos);
    }
}