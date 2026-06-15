using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraRay : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _mousePosition;
    [SerializeField] private InputActionReference _mouseClick;

    [Header("Spring Settings")]
    [SerializeField] private float _springForce = 100f;
    [SerializeField] private float _damper = 10f;
    [SerializeField] private float _drag = 5f;
    [SerializeField] private float _angularDrag = 5f;

    public static event Action<GameObject> OnObjectReleased;

    private Camera _mainCamera;
    private GameObject _draggedObject;
    private Rigidbody _draggedRb;
    private SpringJoint _springJoint;
    private float _dragDistance;

    private float _oldDrag;
    private float _oldAngularDrag;

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
        if (_draggedRb != null && _springJoint != null)
        {
            UpdateSpringTarget();
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

        if (_draggedRb == null)
        {
            _draggedObject = null;
            return;
        }

        _dragDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);

        _oldDrag = _draggedRb.linearDamping; 
        _oldAngularDrag = _draggedRb.angularDamping;
        _draggedRb.linearDamping = _drag;
        _draggedRb.angularDamping = _angularDrag;

        GameObject anchorGO = new GameObject("TempSpringAnchor");
        anchorGO.transform.position = hit.point;
        var anchorRb = anchorGO.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        _springJoint = _draggedObject.AddComponent<SpringJoint>();
        _springJoint.connectedBody = anchorRb;
        
        _springJoint.autoConfigureConnectedAnchor = false;
        _springJoint.anchor = _draggedObject.transform.InverseTransformPoint(hit.point);
        _springJoint.connectedAnchor = Vector3.zero;
        
        _springJoint.spring = _springForce;
        _springJoint.damper = _damper;
    }

    private void UpdateSpringTarget()
    {
        Vector2 mousePos = _mousePosition.action.ReadValue<Vector2>();
        Vector3 targetPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _dragDistance));
        
        if (_springJoint.connectedBody != null)
        {
            _springJoint.connectedBody.MovePosition(targetPos);
        }
    }

    private void ReleaseObject()
    {
        if (_draggedObject == null) return;

        OnObjectReleased?.Invoke(_draggedObject);

        if (_springJoint != null)
        {
            if (_springJoint.connectedBody != null)
            {
                Destroy(_springJoint.connectedBody.gameObject);
            }
            Destroy(_springJoint);
        }

        if (_draggedRb != null)
        {
            _draggedRb.linearDamping = _oldDrag; 
            _draggedRb.angularDamping = _oldAngularDrag;
        }

        _draggedObject = null;
        _draggedRb = null;
    }
}
