using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [Header("SceneLoader")]
    [SerializeField] private SceneLoader _sceneLoader;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Dragble>() != null)
        {
            _sceneLoader.LoadScene(4);
        }
    }
}
