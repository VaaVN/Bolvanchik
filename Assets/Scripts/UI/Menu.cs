using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject[] _stateCam;
    [SerializeField] private GameObject[] _stateFrame;
    [SerializeField] private SceneLoader _sceneLoader;

    public void StartGame()
    {
        _sceneLoader.LoadScene(2);
    }
    public void ChangeState(int state)
    {
        for(int i=0;  i < _stateCam.Length; i++)
        {
            if(i == state)
            {
                _stateCam[i].SetActive(true);
                _stateFrame[i].SetActive(true);
            }
            else
            {
                _stateCam[i].SetActive(false);
                _stateFrame[i].SetActive(false);
            }
        }
    }
    public void Exit()
    {
        Application.Quit();
    }
}
