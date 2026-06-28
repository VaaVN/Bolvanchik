using System;
using UnityEngine;

public class Door :  InteractableObject
{
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        _animator.Play("Open");
    }
}
