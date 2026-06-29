using UnityEngine;
using System;

public class Dragble : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private AudioSource _audioSource;
    
    [Header("Prefs")]
    [SerializeField] private float _dragForce = 100f;
    [SerializeField] private float _velocityDamper = 10f;
    [SerializeField] private float _drag = 15f;
    [SerializeField] private float _angularDrag = 5f;
    
    [Header("StaminaPrefs")]
    [SerializeField] private bool _useStamina = false;
    [SerializeField] private float _staminaTime = 5f;
    [SerializeField] private int _staminaMaterialIndex = 1;

    float _stamina;
    private Material _staminaMaterial;

    private Rigidbody rb;
    private MaterialPropertyBlock _block;
    private Rock _attachedrock,_ignoredrock;
    private float _ignoreUntil;
    private ConfigurableJoint _attachJoint;
    private LimbState _curState;
    private bool _isHovered;
    
    public bool IsAttached => _curState == LimbState.Attached;
    public bool IsTired => _curState == LimbState.Tired;

    private void Awake()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer != null && renderer.materials.Length > _staminaMaterialIndex)
        {
            _staminaMaterial = renderer.materials[_staminaMaterialIndex];
        }
        
        rb = GetComponent<Rigidbody>();
        _block = new MaterialPropertyBlock();
    }

    public void OnGrab()
    {
        if (_curState == LimbState.Tired)
            return;
        rb.linearDamping = _drag;
        rb.angularDamping = _angularDrag;
        if(_curState == LimbState.Attached) Detach();
        SetState(LimbState.Dragging);
    }

    public void OnRelease()
    {
        if (_curState != LimbState.Dragging)
            return;

        SetState(LimbState.Free);
    }


    public void ApplyDragForce(Vector3 targetPos)
    {
        Vector3 delta = targetPos - rb.position;

        Vector3 force =
            delta * _dragForce -
            rb.linearVelocity * _velocityDamper;

        rb.AddForce(force);
    }

    public void SetState(LimbState state)
    {
        _curState = state;
        ChangeVisual();
    }

    public void SetHover(bool value)
    {
        _isHovered = value;
        ChangeVisual();
    }
    public void ChangeVisual()
    {
        Color color;
        foreach (Renderer r in rb.GetComponentsInChildren<Renderer>())
        {
            if (!r)
                continue;

            r.GetPropertyBlock(_block, 0);
            _block.Clear();
            switch (_curState)
            {
                case LimbState.Dragging:
                    color = Color.blue;
                    break;
                case LimbState.Attached:
                    if (_isHovered)
                    {
                        color = Color.yellow;
                        break;
                    }
                    color = Color.green;
                    break;
                case LimbState.Tired:
                    color = Color.red;
                    break;
                default:
                    if (_isHovered)
                    {
                        color = Color.yellow;
                        break;
                    }
                    color = Color.white;
                    break;
            }
            
            if (r.sharedMaterial.HasProperty("_BaseColor"))
                _block.SetColor("_BaseColor", color);

            else if (r.sharedMaterial.HasProperty("_Color"))
                _block.SetColor("_Color", color);

            r.SetPropertyBlock(_block, 0);
        }
    }
    
    public bool CanAttach(Rock rock)
    {
        if (_ignoredrock == rock &&
            Time.time < _ignoreUntil)
            return false;

        return _curState == LimbState.Dragging;
    }
    public void Attach(Rock rock)
    {
        _attachedrock = rock;
        CreateAttachJoint(rock);
        _attachedrock.AttachLimb();
        SetState(LimbState.Attached);
        _audioSource.Play();
    }

    public void Detach()
    {
        _ignoreUntil = Time.time + 1f;
        _ignoredrock = _attachedrock;
        DestroyAttachJoint();
        _attachedrock.DetachLimb();
        _attachedrock = null;
        SetState(LimbState.Free);
    }
    private void CreateAttachJoint(Rock rock)
    {
        _attachJoint = gameObject.AddComponent<ConfigurableJoint>();

        _attachJoint.connectedBody = rock.GetComponent<Rigidbody>();
        _attachJoint.xMotion = ConfigurableJointMotion.Locked;
        _attachJoint.yMotion = ConfigurableJointMotion.Locked;
        _attachJoint.zMotion = ConfigurableJointMotion.Locked;
        _attachJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        _attachJoint.angularXMotion = ConfigurableJointMotion.Free;
        _attachJoint.angularYMotion = ConfigurableJointMotion.Free;
        _attachJoint.angularZMotion = ConfigurableJointMotion.Free;
        _attachJoint.projectionDistance = 0.01f;
        _attachJoint.projectionAngle = 1f;

    }
    private void DestroyAttachJoint()
    {
        if (_attachJoint != null)
        {
            _attachJoint.connectedBody = null;
            Destroy(_attachJoint);
            _attachJoint = null;
        }
    }
    public void ChangeStamina(float delta)
    {
        if (!_useStamina)
            return;
    
        _stamina += delta;
        _stamina = Mathf.Clamp(_stamina, 0f, 100f);
    
        UpdateStaminaVisual();
    
        if (_curState != LimbState.Tired &&
            _stamina >= 100f)
        {
            if (_curState == LimbState.Attached)
                Detach();
    
            SetState(LimbState.Tired);
            return;
        }
    
        if (_curState == LimbState.Tired &&
            _stamina <= 0f)
        {
            SetState(LimbState.Free);
        }
    }
    public void Stamina(float multiplier)
    {
        ChangeStamina(
            Time.deltaTime *
            (100f / _staminaTime) *
            multiplier);
    }

    public void Recover()
    {
        ChangeStamina(
            -Time.deltaTime *
            (100f / _staminaTime));
    }
    private void UpdateStaminaVisual()
    {
        if (_staminaMaterial == null)
            return;

        float alpha = _stamina / 100f;

        _staminaMaterial.SetFloat("_Transparency", alpha);
    }
}