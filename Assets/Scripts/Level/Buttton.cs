using System;
using UnityEngine;

public class Buttton : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private InteractableObject _interactableObject;
    
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Dragble>() != null)
        {
            _interactableObject.Interact();
            _animator.Play("Interact");
        }
    }
}
