using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

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

    [Header("UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject finishUI;
    [SerializeField] private GameObject playUI;
    [SerializeField] private Text timerText;
    [SerializeField] private Slider RPMSlider;
    [SerializeField] private Text speedText;
    [SerializeField] private TMP_Text totalTimeText;

    [Header("Timer")]
    private float totalSeconds;
    private string s_seconds;
    private string s_minutes;

    [Header("References")]
    [SerializeField] private GameObject finishLinePrefab;
    [SerializeField] private LayerMask finishLineLayerMask;
    [SerializeField] private CarScript car;

    private Transform instantiatedFinishLine;
    private void Start()
    {
        Resume();
        Transform instantiatedMap = Instantiate(DataBetweenScenes.mapSelected.prefab).transform;
        instantiatedFinishLine = Instantiate(finishLinePrefab, instantiatedMap).transform;
        instantiatedFinishLine.position = DataBetweenScenes.mapSelected.finishLine.position;
        instantiatedFinishLine.rotation = Quaternion.Euler( DataBetweenScenes.mapSelected.finishLine.eulerAngles );
        car.transform.position = DataBetweenScenes.mapSelected.spawnPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameState == GameStates.PauseState) Resume();
            else if (gameState == GameStates.PlayState) Pause();
        }

        if (Physics.OverlapBox(instantiatedFinishLine.position, instantiatedFinishLine.GetChild(1).GetComponent<MeshFilter>().mesh.bounds.size, instantiatedFinishLine.transform.rotation, finishLineLayerMask).Length > 0)
        {
            Finish();
        }

        RunTimer();

        speedText.text = Mathf.Floor( car.kmph )+ "km/h";
        RPMSlider.value = car.rpm;
    }

    void RunTimer()
    {
        totalSeconds += Time.deltaTime;

        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds - minutes * 60);

        s_minutes = minutes / 10 > 0 ? minutes.ToString() : "0" + minutes.ToString();
        s_seconds = seconds / 10 > 0 ? seconds.ToString() : "0" + seconds.ToString();

        timerText.text = s_minutes + " : " + s_seconds;
    }


    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameState = GameStates.PlayState;
        Time.timeScale = 1;
        pauseUI.SetActive(false);
    }
    private void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameState = GameStates.PauseState;
        Time.timeScale = 0;
        pauseUI.SetActive(true);

    }
    
    private void Finish()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        totalTimeText.text = s_minutes + " : " + s_seconds;
        gameState = GameStates.WinState;
        finishUI.SetActive(true);
        playUI.SetActive(false);
        Time.timeScale = 0;
    }


}
