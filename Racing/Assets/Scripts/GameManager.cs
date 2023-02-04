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
    [SerializeField] private Text speedText;
    [SerializeField] private TMP_Text totalTimeText;

    [Header("Finish line")]
    [SerializeField] private GameObject finishLine;
    [SerializeField] private LayerMask finishLineLayerMask;


    [Header("Timer")]
    private float totalSeconds;
    [SerializeField] private string s_seconds;
    [SerializeField] private string s_minutes;

    [Header("References")]
    [SerializeField] private Car car;
    [SerializeField] private CinemachineVirtualCamera cinecam;

    private const float maxFov = 90;
    private float minFov;

    private void Awake()
    {
        minFov = cinecam.m_Lens.FieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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

        RunTimer();

        cinecam.m_Lens.FieldOfView = minFov + (Mathf.Abs(car.kmph) / car.carDataScriptableObject.topSpeed) * (maxFov - minFov);

        speedText.text = Mathf.Floor( car.kmph )+ "km/h";
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
        totalTimeText.text = s_minutes + " : " + s_seconds;
        gameState = GameStates.WinState;
        finishUI.SetActive(true);
        playUI.SetActive(false);
        Time.timeScale = 0;
    }


}
