using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _openMenu;
    
    [Header("Ref")]
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _pauseMenu;

    private bool _isOpen;
    private void OnEnable()
    {
        _openMenu.action.Enable();
        _openMenu.action.started += OnOpen;
    }

    private void OnDisable()
    {
        _openMenu.action.started -= OnOpen;
        _openMenu.action.Disable();
    }
    private void OnOpen(InputAction.CallbackContext context)
    {
        ChangeState(!_isOpen);
    }

    public void ChangeState(bool open)
    {
        _isOpen = open;
        _player.SetActive(!open);
        _pauseMenu.SetActive(open);
    }
}
