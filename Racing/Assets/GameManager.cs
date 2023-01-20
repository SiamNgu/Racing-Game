using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum GameStates
    {
        StartState,
        PauseState, 
        PlayState,
        WinState
    }

    private GameStates gameState = GameStates.PlayState;

    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject finishUI;
    [SerializeField] private GameObject playUI;

    [SerializeField] private GameObject finishLine;
    [SerializeField] private LayerMask finishLineLayerMask;

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

        if (Physics.OverlapBox(finishLine.transform.position, finishLine.GetComponent<MeshFilter>().mesh.bounds.size, finishLine.transform.rotation, finishLineLayerMask).Length > 0)
        {
            Finish();
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
        gameState = GameStates.WinState;
        finishUI.SetActive(true);
        playUI.SetActive(false);
        Time.timeScale = 0;
    }


}
