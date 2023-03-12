using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        Time.timeScale = 1.0f;
        if (sceneName.Length == 0) return;  
        SceneManager.LoadScene(sceneName);
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadScene);
        for (int i = 0; i < GetComponent<Button>().onClick.GetPersistentEventCount(); i++)
        {
            GetComponent<Button>().onClick.GetPersistentMethodName(i);
        }
    }
}
