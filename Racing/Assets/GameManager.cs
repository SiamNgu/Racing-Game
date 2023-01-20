using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum GameStates
    {
        PauseState, 
        PlayState,
        WinState
    }

    private GameStates gameState = GameStates.PlayState;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject finishLine;

    private void Start()
    {
        Resume();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameState == GameStates.PauseState) Resume();
            else if (gameState == GameStates.PlayState) Pause();
        }
    }

    public void Resume()
    {
        gameState = GameStates.PlayState;
        Time.timeScale = 1;
        pauseUI.SetActive(false);
    }
    private void Pause()
    {
        gameState = GameStates.PauseState;
        Time.timeScale = 0;
        pauseUI.SetActive(true);
    }
    
    private void Finish()
    {

    }
}
