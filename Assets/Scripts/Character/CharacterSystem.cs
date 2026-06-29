using System.Collections.Generic;
using UnityEngine;
public class CharacterSystem : MonoBehaviour
{
    [Header("Character Parts")]
    [SerializeField] private List<Dragble> Legs;
    [SerializeField] private List<Dragble> Hands;

    [Header("Ref")]
    [SerializeField] private CameraRay ray;
    [SerializeField] private Rigidbody _mainBody;

    [Header("Prefs")]
    [SerializeField] private float _maxLimbDistance = 1.5f;
    
    [Header("Respawn")]
    [SerializeField] private Transform _spawnPoint;

    private Dragble _draggedObject, _hoveredObject;
    private PointerHitData? _currentDrag;

    private Rigidbody[] _rigidbodies;
    

    private void Start()
    {
        ray.ClickStarted += OnClickStarted;
        ray.ClickReleased += OnClickReleased;
        ray.HoverObject += OnHoverObject;

        _rigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    private void OnDestroy()
    {
        ray.ClickStarted -= OnClickStarted;
        ray.ClickReleased -= OnClickReleased;
        ray.HoverObject -= OnHoverObject;
    }
    private void OnClickStarted(PointerHitData data)
    {
        ClearHover();
        Dragble dragble =
            data.Hit.transform.GetComponentInParent<Dragble>();

        if (dragble == null)
            return;

        _draggedObject = dragble;

        _currentDrag = data;

        _draggedObject.OnGrab();
    }

    private void OnClickReleased()
    {
        if (_draggedObject == null)
            return;

        _draggedObject.OnRelease();

        _draggedObject = null;
        _currentDrag = null;
    }


    private void DragObject()
    {
        Vector2 pointerPos = ray.PointerPosition;

        Vector3 targetPos =
            ray.MainCamera.ScreenToWorldPoint(
                new Vector3(
                    pointerPos.x,
                    pointerPos.y,
                    _currentDrag.Value.DragDistance));

        Vector3 offset = targetPos - _mainBody.position;

        if (offset.magnitude > _maxLimbDistance)
        {
            targetPos =
                _mainBody.position +
                offset.normalized * _maxLimbDistance;
        }

        _draggedObject.ApplyDragForce(targetPos);
    }

    private void Update()
    {
        UpdateStamina();
        if (_draggedObject == null)
            return;
        DragObject();
    }

    public void TryAttach(Rock rock, Dragble dragble)
    {
        if (!dragble.CanAttach(rock))
            return;
        dragble.Attach(rock);
        OnClickReleased();
    }

    private void OnHoverObject(PointerHoverData? data)
    {
        if (data == null)
        {
            ClearHover();
            return;
        }
        Dragble hoverObject = data.Value.Hit.transform.GetComponentInParent<Dragble>();

        if (hoverObject == null)
        {
            ClearHover();
            return;
        }

        if (_hoveredObject != hoverObject)
        {
            ClearHover();

            _hoveredObject = hoverObject;
            _hoveredObject.SetHover(true);
        }
    }

    private void ClearHover()
    {
        if (_hoveredObject == null)
        {
            return;
        }
        _hoveredObject.SetHover(false);
        _hoveredObject = null;
    }

    public void Respawn()
    {
        OnClickReleased();
        Vector3 delta = _spawnPoint.position - _mainBody.position;

        foreach (Rigidbody rb in _rigidbodies)
        {
            rb.position += delta;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
        
        _mainBody.rotation = _spawnPoint.rotation;
        
        foreach (Rigidbody rb in _rigidbodies)
            rb.WakeUp();
    }
    private void UpdateStamina()
    {
        int attachedLegs = 0;
        foreach (Dragble leg in Legs)
        {
            if (leg.IsAttached)
                attachedLegs++;
            
        }
        foreach (Dragble hand in Hands)
        {
            if (hand.IsTired)
            {
                hand.Recover();
                Debug.Log("Recov1");
                continue;
            }
            if (!hand.IsAttached)
            {
                hand.Recover();
                Debug.Log("Recov1");
                continue;
            }
            switch (attachedLegs)
            {
                case 0:
                    hand.Stamina(1f);
                    Debug.Log("0");
                    break;
                case 1:
                    hand.Stamina(0.5f);
                    Debug.Log("1");
                    break;
                case 2:
                    hand.Recover();
                    Debug.Log("2");
                    break;
            }
        }
    }
}