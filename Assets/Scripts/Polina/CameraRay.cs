using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRay : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _mousePosition;
    [SerializeField] private InputActionReference _mouseClick;

    private Camera _mainCamera;
    private GameObject _draggedObject;
    private Rigidbody _draggedRb;
    private Renderer _draggedRenderer;
    private float _dragDistance;

    private Vector3 _targetPosition;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _mousePosition.action.Enable();
        _mouseClick.action.Enable();
        _mouseClick.action.started += OnClickStarted;
        _mouseClick.action.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        _mousePosition.action.Disable();
        _mouseClick.action.Disable();
        _mouseClick.action.started -= OnClickStarted;
        _mouseClick.action.canceled -= OnClickCanceled;
        ReleaseObject();
    }

    private void FixedUpdate()
    {
        if (_draggedObject != null && _draggedRb != null)
        {
            DragObject();
        }
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_draggedObject != null) return;

        Vector2 mousePos = _mousePosition.action.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            var draggable = hit.transform.GetComponent<Dragble>();
            if (draggable != null)
            {
                StartDrag(hit);
            }
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        ReleaseObject();
    }

    private void StartDrag(RaycastHit hit)
    {
        _draggedObject = hit.transform.gameObject;
        _draggedRb = hit.transform.GetComponent<Rigidbody>();
        _draggedRenderer = hit.transform.GetComponent<Renderer>();

        if (_draggedRb == null)
        {
            _draggedObject = null;
            return;
        }

        _draggedRb.isKinematic = false;
        _draggedRb.WakeUp();
        _dragDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);
    }

    private void DragObject()
    {
        Vector2 mousePos = _mousePosition.action.ReadValue<Vector2>();
        _targetPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _dragDistance));

        _draggedRb.MovePosition(_targetPosition);
    }

    private void ReleaseObject()
    {
        if (_draggedObject == null) return;

        _draggedObject = null;
        _draggedRb = null;
        _draggedRenderer = null;
    }
}