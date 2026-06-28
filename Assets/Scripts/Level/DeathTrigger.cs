using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] CharacterSystem _characterSystem;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Dragble>() != null)
        {
            _characterSystem.Respawn();
        }
    }
}
