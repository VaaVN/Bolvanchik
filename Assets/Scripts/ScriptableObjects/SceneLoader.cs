using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneLoader", menuName = "Scriptable Objects/SceneLoader")]
public class SceneLoader : ScriptableObject
{
    [SerializeField] private int _loadingSceneIndex;

    public async void LoadScene(int sceneIndex)
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(_loadingSceneIndex, LoadSceneMode.Single);
        await loadScene;
        
        LoadingManager manager = FindFirstObjectByType<LoadingManager>();
        AsyncOperation openScene = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        while(!openScene.isDone)
        {
            manager.SetPercent(openScene.progress);
            await Task.Delay(300);
        }
        await SceneManager.UnloadSceneAsync(_loadingSceneIndex);

    }
}
