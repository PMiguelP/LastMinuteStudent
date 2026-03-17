using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverScoreText;

    [Header("Characters")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator chaserAnimator;
    [SerializeField] private string runParameterName = "IsRunning";

    [Header("Gameplay")]
    [SerializeField] private Behaviour[] behavioursToEnableOnStart;
    [SerializeField] private bool pauseTimeWhileMenuIsOpen = true;
    [SerializeField] private float scorePerSecond = 10f;

    private float currentScore;
    private bool gameIsRunning;
    private bool gameIsOver;

    private void Start()
    {
        ShowMenu();
    }

    private void Update()
    {
        HandleMenuInput();

        if (!gameIsRunning)
        {
            return;
        }

        currentScore += scorePerSecond * Time.deltaTime;
        UpdateScoreUI();
    }

    public void SetMainMenuPanel(GameObject panel)
    {
        mainMenuPanel = panel;
    }

    public void SetGameOverPanel(GameObject panel)
    {
        gameOverPanel = panel;
    }

    public void SetScoreText(Text textComponent)
    {
        scoreText = textComponent;
        UpdateScoreUI();
    }

    public void SetScorePanel(GameObject panel)
    {
        scorePanel = panel;
    }

    public void SetGameOverScoreText(Text textComponent)
    {
        gameOverScoreText = textComponent;
        UpdateScoreUI();
    }

    public GameObject GetMainMenuPanel()
    {
        return mainMenuPanel;
    }

    public GameObject GetGameOverPanel()
    {
        return gameOverPanel;
    }

    public bool IsGameRunning()
    {
        return gameIsRunning;
    }

    public void StartGame()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
        }

        currentScore = 0f;
        gameIsRunning = true;
        gameIsOver = false;
        ResetRunnerControllers();
        UpdateScoreUI();

        UpdateRunnerAnimations(true);
        SetAnimatorRunning(chaserAnimator, true);
        SetBehavioursEnabled(true);

        if (pauseTimeWhileMenuIsOpen)
        {
            Time.timeScale = 1f;
        }
    }

    public void ShowMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }

        currentScore = 0f;
        gameIsRunning = false;
        gameIsOver = false;
        UpdateScoreUI();

        UpdateRunnerAnimations(false);
        SetAnimatorRunning(chaserAnimator, false);
        SetBehavioursEnabled(false);

        if (pauseTimeWhileMenuIsOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void ShowGameOver()
    {
        if (gameIsOver)
        {
            return;
        }

        gameIsRunning = false;
        gameIsOver = true;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }

        UpdateRunnerAnimations(false);
        SetAnimatorRunning(chaserAnimator, false);
        SetBehavioursEnabled(false);
        UpdateScoreUI();

        if (pauseTimeWhileMenuIsOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetAnimatorRunning(Animator animatorToUpdate, bool isRunning)
    {
        if (animatorToUpdate == null)
        {
            return;
        }

        animatorToUpdate.SetBool(runParameterName, isRunning);
        animatorToUpdate.Update(0f);
    }

    private void SetBehavioursEnabled(bool isEnabled)
    {
        if (behavioursToEnableOnStart == null)
        {
            return;
        }

        foreach (Behaviour behaviour in behavioursToEnableOnStart)
        {
            if (behaviour != null)
            {
                behaviour.enabled = isEnabled;
            }
        }
    }

    private void HandleMenuInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        bool startPressed = keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;

        if (!gameIsRunning && !gameIsOver && mainMenuPanel != null && mainMenuPanel.activeSelf && startPressed)
        {
            StartGame();
            return;
        }

        if (gameIsOver && gameOverPanel != null && gameOverPanel.activeSelf && startPressed)
        {
            RestartGame();
        }
    }

    private void UpdateScoreUI()
    {
        string scoreValue = Mathf.FloorToInt(currentScore).ToString("0000");

        if (scoreText != null)
        {
            scoreText.text = scoreValue;
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = scoreValue;
        }
    }

    private void ResetRunnerControllers()
    {
        PlayerRunnerController[] runnerControllers = FindObjectsByType<PlayerRunnerController>(FindObjectsSortMode.None);

        foreach (PlayerRunnerController runnerController in runnerControllers)
        {
            if (runnerController != null)
            {
                runnerController.ResetRunnerToStart();
            }
        }

        ChaserFollower[] chaserFollowers = FindObjectsByType<ChaserFollower>(FindObjectsSortMode.None);

        foreach (ChaserFollower chaserFollower in chaserFollowers)
        {
            if (chaserFollower != null)
            {
                chaserFollower.ResetChaserToStart();
            }
        }
    }

    private void UpdateRunnerAnimations(bool isRunning)
    {
        PlayerRunnerController[] runnerControllers = FindObjectsByType<PlayerRunnerController>(FindObjectsSortMode.None);

        foreach (PlayerRunnerController runnerController in runnerControllers)
        {
            if (runnerController != null)
            {
                runnerController.SetRunningAnimation(isRunning);
            }
        }

        SetAnimatorRunning(playerAnimator, isRunning);
    }
}
