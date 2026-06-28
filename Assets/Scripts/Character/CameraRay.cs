using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRay : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _mouseClick;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;

    public Vector2 PointerPosition =>
        Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

    public Camera MainCamera => _mainCamera;

    public event Action<PointerHitData> ClickStarted;
    public event Action<PointerHoverData?> HoverObject;
    public event Action ClickReleased;
    

    private void Awake()
    {
        if (!_mainCamera)
            _mainCamera = GetComponent<Camera>();
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
    }

    private void Update()
    {
        SendHover();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (!Physics.Raycast(
                _mainCamera.ScreenPointToRay(PointerPosition),
                out RaycastHit hit))
            return;

        float dragDistance = Vector3.Distance(
            _mainCamera.transform.position,
            hit.point);

        PointerHitData data = new(hit, dragDistance);

        ClickStarted?.Invoke(data);
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        ClickReleased?.Invoke();
    }

    private void SendHover()
    {
        if (Physics.Raycast(
                _mainCamera.ScreenPointToRay(PointerPosition),
                out RaycastHit hit))
        {
            PointerHoverData data = new(hit);
            
            HoverObject?.Invoke(data);
        }
        else
        {
            HoverObject?.Invoke(null);
        }
    }
    
}