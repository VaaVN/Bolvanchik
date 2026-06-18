using UnityEngine;
using System;

public class Dragble : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float _dragForce = 100f;
    [SerializeField] private float _velocityDamper = 10f;
    [SerializeField] private float _drag = 15f;
    [SerializeField] private float _angularDrag = 5f;

    private Rigidbody rb;
    private MaterialPropertyBlock _block;
    private float _oldDrag;
    private float _oldAngularDrag;

    private Camera _mainCamera;
    private float _dragDistance;
    private bool _isDragged;
    public static event Action<GameObject> OnObjectReleased;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        _block = new MaterialPropertyBlock();
        _mainCamera = Camera.main;
    }

    public void OnGrab(float dragDistance)
    {
        _dragDistance = dragDistance;

        _oldDrag = rb.linearDamping;
        _oldAngularDrag = rb.angularDamping;

        rb.linearDamping = _drag;
        rb.angularDamping = _angularDrag;

        SetHighlight(true, Color.blue);
        _isDragged = true;
    }

    public void OnRelease()
    {
        if (!_isDragged) return;

        OnObjectReleased?.Invoke(gameObject);

        rb.linearDamping = _oldDrag;
        rb.angularDamping = _oldAngularDrag;

        SetHighlight(false);
        _isDragged = false; 
    }

    public void ApplyDragForce(Vector3 targetPos)
    {
        Vector3 delta = targetPos - rb.position;
        Vector3 force = delta * _dragForce - rb.linearVelocity * _velocityDamper;
        rb.AddForce(force);
    }

    public void SetHover(bool isActive)
    {
        SetHighlight(isActive, Color.green);
    }

    public void SetHighlight(bool active, Color color = new())
    {
        foreach (var r in rb.GetComponentsInChildren<Renderer>())
        {
            if (!r) continue;

            r.GetPropertyBlock(_block);

            if (active)
            {
                if (r.sharedMaterial.HasProperty("_BaseColor")) _block.SetColor("_BaseColor", color);
                else if (r.sharedMaterial.HasProperty("_Color")) _block.SetColor("_Color", color);
            }
            else
            {
                _block.Clear();
            }

            r.SetPropertyBlock(_block);
        }
    }
}