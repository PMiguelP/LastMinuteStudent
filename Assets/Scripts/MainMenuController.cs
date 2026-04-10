using System.Collections;
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
    [SerializeField] private Text coinText;
    [SerializeField] private Text gameOverCoinText;

    [Header("Characters")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator chaserAnimator;
    [SerializeField] private string runParameterName = "IsRunning";

    [Header("Gameplay")]
    [SerializeField] private Behaviour[] behavioursToEnableOnStart;
    [SerializeField] private bool pauseTimeWhileMenuIsOpen = true;
    [SerializeField] private float scorePerSecond = 10f;
    [SerializeField] private RunnerChunkRecycler chunkRecycler;

    private float currentScore;
    private int currentCoins;
    private bool gameIsRunning;
    private bool gameIsOver;

    private float _highScore;
    private int   _lifetimeCoins;
    private Image _proximityBarFill;
    private Text _proximityLabel;
    private Text _highScoreText;
    private Text _lifetimeCoinText;
    private CanvasGroup _loadingOverlay;

    private int _lastDisplayedScore = -1;
    private PlayerRunnerController[] _runners;
    private ChaserFollower[]         _chasers;

    private void Awake()
    {
        _highScore    = PlayerPrefs.GetFloat("HighScore", 0f);
        _lifetimeCoins = PlayerPrefs.GetInt("LifetimeCoins", 0);
    }

    private void Start()
    {
        chunkRecycler ??= FindFirstObjectByType<RunnerChunkRecycler>();
        _runners = FindObjectsByType<PlayerRunnerController>(FindObjectsSortMode.None);
        _chasers = FindObjectsByType<ChaserFollower>(FindObjectsSortMode.None);

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

    // ─── Setters called by UIBuilder ──────────────────────────────────────────

    public void SetMainMenuPanel(GameObject panel) => mainMenuPanel = panel;
    public void SetGameOverPanel(GameObject panel) => gameOverPanel = panel;
    public void SetScorePanel(GameObject panel) => scorePanel = panel;
    public void SetHighScoreText(Text text) => _highScoreText = text;

    public void SetScoreText(Text textComponent)
    {
        scoreText = textComponent;
        UpdateScoreUI();
    }

    public void SetGameOverScoreText(Text textComponent)
    {
        gameOverScoreText = textComponent;
        UpdateScoreUI();
    }

    public void SetCoinText(Text textComponent)
    {
        coinText = textComponent;
        UpdateCoinUI();
    }

    public void SetGameOverCoinText(Text textComponent)
    {
        gameOverCoinText = textComponent;
        UpdateCoinUI();
    }

    public void SetProximityBarFill(Image img) => _proximityBarFill = img;
    public void SetProximityLabel(Text label) => _proximityLabel = label;
    public void SetLifetimeCoinText(Text text) => _lifetimeCoinText = text;
    public void SetLoadingScreen(CanvasGroup overlay) => _loadingOverlay = overlay;

    public void ClearUIRefs()
    {
        mainMenuPanel = null;
        gameOverPanel = null;
        scorePanel = null;
        scoreText = null;
        gameOverScoreText = null;
        coinText = null;
        gameOverCoinText = null;
    }

    // ─── Getters ──────────────────────────────────────────────────────────────

    public GameObject GetMainMenuPanel() => mainMenuPanel;
    public GameObject GetGameOverPanel() => gameOverPanel;
    public GameObject GetScorePanel() => scorePanel;
    public bool IsGameRunning() => gameIsRunning;
    public float GetScore() => currentScore;
    public int GetCoins() => currentCoins;
    public int GetLifetimeCoins() => _lifetimeCoins;

    // ─── Score / Coins ────────────────────────────────────────────────────────

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        currentScore += amount;
        UpdateScoreUI();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        currentCoins += amount;
        UpdateCoinUI();
    }

    // ─── Chaser proximity ─────────────────────────────────────────────────────

    // t = 0 danger, t = 1 safe
    public void SetChaserProximity(float t)
    {
        if (_proximityBarFill != null)
        {
            _proximityBarFill.fillAmount = t;
            _proximityBarFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.8f, 0.3f), t);
        }

        if (_proximityLabel != null)
        {
            _proximityLabel.enabled = t < 0.35f;
        }
    }

    // ─── Catch sequence ───────────────────────────────────────────────────────

    // Called by ChaserFollower: stops player without freezing time or showing panel
    public void BeginCatchSequence()
    {
        gameIsRunning = false;
    }

    // ─── Game flow ────────────────────────────────────────────────────────────

    public void StartGame()
    {
        GameAudioManager.Instance?.Play(SoundEvent.StartGame);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (scorePanel != null)   scorePanel.SetActive(true);

        currentScore = 0f;
        currentCoins = 0;
        _lastDisplayedScore = -1;
        gameIsRunning = true;
        gameIsOver = false;
        ResetRunnerControllers();
        UpdateScoreUI();
        UpdateCoinUI();

        chunkRecycler?.ResetChunks();

        UpdateRunnerAnimations(true);
        SetAnimatorRunning(chaserAnimator, true);
        SetBehavioursEnabled(true);

        // Reset proximity bar to safe
        SetChaserProximity(1f);

        if (pauseTimeWhileMenuIsOpen)
        {
            Time.timeScale = 1f;
        }
    }

    public void ShowMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (scorePanel != null)   scorePanel.SetActive(false);

        currentScore = 0f;
        currentCoins = 0;
        gameIsRunning = false;
        gameIsOver = false;
        UpdateScoreUI();
        UpdateCoinUI();

        if (chunkRecycler != null)
        {
            chunkRecycler.ResetChunks();
        }
        else
        {
            RespawnChunkSpawners();
        }

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

        // Track high score
        if (currentScore > _highScore)
        {
            _highScore = currentScore;
            PlayerPrefs.SetFloat("HighScore", _highScore);
        }

        _lifetimeCoins += currentCoins;
        PlayerPrefs.SetInt("LifetimeCoins", _lifetimeCoins);
        PlayerPrefs.Save();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        UpdateRunnerAnimations(false);
        SetAnimatorRunning(chaserAnimator, false);
        SetBehavioursEnabled(false);
        UpdateScoreUI();
        UpdateCoinUI();
        UpdateHighScoreUI();

        if (pauseTimeWhileMenuIsOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartGame() => StartCoroutine(RestartWithFade());

    private IEnumerator RestartWithFade()
    {
        if (_loadingOverlay != null)
        {
            _loadingOverlay.gameObject.SetActive(true);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.35f;
                _loadingOverlay.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            _loadingOverlay.alpha = 1f;
        }

        ShowMenu();
        yield return null;

        if (_loadingOverlay != null)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime / 0.4f;
                _loadingOverlay.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            _loadingOverlay.alpha = 0f;
            _loadingOverlay.gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─── Internal helpers ─────────────────────────────────────────────────────

    private void SetAnimatorRunning(Animator animatorToUpdate, bool isRunning)
    {
        if (animatorToUpdate == null) return;
        animatorToUpdate.SetBool(runParameterName, isRunning);
        animatorToUpdate.Update(0f);
    }

    private void SetBehavioursEnabled(bool isEnabled)
    {
        if (behavioursToEnableOnStart == null) return;

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
        if (keyboard == null) return;

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
        int score = Mathf.FloorToInt(currentScore);
        if (score != _lastDisplayedScore)
        {
            _lastDisplayedScore = score;
            if (scoreText != null) scoreText.text = score.ToString();
        }
        if (gameOverScoreText != null) gameOverScoreText.text = score.ToString("0000");
    }

    private void UpdateCoinUI()
    {
        if (coinText != null) coinText.text = $"\u25cf {currentCoins}";
        if (gameOverCoinText != null) gameOverCoinText.text = currentCoins.ToString();
        if (_lifetimeCoinText != null) _lifetimeCoinText.text = _lifetimeCoins.ToString();
    }

    private void UpdateHighScoreUI()
    {
        if (_highScoreText == null) return;
        bool isNewRecord = currentScore >= _highScore && currentScore > 0f;
        _highScoreText.text = isNewRecord
            ? $"★ NEW BEST  {Mathf.FloorToInt(_highScore):0000}"
            : $"Best  {Mathf.FloorToInt(_highScore):0000}";
        _highScoreText.color = isNewRecord ? new Color(0.98f, 0.84f, 0.28f) : new Color(0.7f, 0.75f, 0.8f);
    }

    private void ResetRunnerControllers()
    {
        foreach (PlayerRunnerController runner in _runners)
            runner?.ResetRunnerToStart();

        foreach (ChaserFollower chaser in _chasers)
            chaser?.ResetChaserToStart();
    }

    private void UpdateRunnerAnimations(bool isRunning)
    {
        foreach (PlayerRunnerController runner in _runners)
            runner?.SetRunningAnimation(isRunning);

        SetAnimatorRunning(playerAnimator, isRunning);
    }

    private void RespawnChunkSpawners()
    {
        foreach (ChunkSpawnController spawner in FindObjectsByType<ChunkSpawnController>(FindObjectsSortMode.None))
        {
            spawner?.SpawnIntoChunk();
        }
    }
}
