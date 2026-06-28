using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _rightMouseClick;
    private void OnEnable()
    {
        _rightMouseClick.action.Enable();
        _rightMouseClick.action.started += OnClickStarted;
        _rightMouseClick.action.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        _rightMouseClick.action.started -= OnClickStarted;
        _rightMouseClick.action.canceled -= OnClickCanceled;
        _rightMouseClick.action.Disable();
    }
    private void OnClickStarted(InputAction.CallbackContext context)
    {
        transform.GetComponent<CinemachineInputAxisController>().enabled = true;
    }
    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        transform.GetComponent<CinemachineInputAxisController>().enabled = false;
    }
}
