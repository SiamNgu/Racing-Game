using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (sceneName.Length == 0) return;
        SceneManager.LoadScene(sceneName);
    }
}
