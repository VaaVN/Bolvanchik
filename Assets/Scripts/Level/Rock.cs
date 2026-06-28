using System;
using UnityEngine;

public class Rock : MonoBehaviour, IHoldable
{
    [Header("Ref")]
    [SerializeField] CharacterSystem _characterSystem;

    private bool _isBusy;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Dragble dragble) || _isBusy)
            return;

        PullRequest(dragble);
    }

    public void DetachLimb()
    {
        _isBusy = false;  
    }
    public void AttachLimb()
    {
        _isBusy = true;   
    }
    public void PullRequest(Dragble limb)
    {
        _characterSystem.TryAttach(this, limb);
    }
}
