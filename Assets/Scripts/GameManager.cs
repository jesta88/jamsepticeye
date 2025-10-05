using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 120f; // 2 minutes
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private CanvasGroup winPanel;
    [SerializeField] private CanvasGroup losePanel;

    private float _timeRemaining;
    private int _score;
    private bool _gameActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!_gameActive) return;

        _timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (_timeRemaining <= 0)
        {
            EndGame(false);
        }
    }

    public void StartGame()
    {
        _timeRemaining = gameDuration;
        _score = 0;
        _gameActive = true;
        
        if (winPanel) winPanel.alpha = 0f;
        if (losePanel) losePanel.alpha = 0f;
        
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        _score += points;
        UpdateScoreUI();
        
        // Win condition: fix certain number of objects
        if (_score >= 10)
        {
            EndGame(true);
        }
    }

    private void EndGame(bool won)
    {
        _gameActive = false;
        
        if (won && winPanel)
        {
            winPanel.alpha = 1f;
        }
        else if (!won && losePanel)
        {
            losePanel.alpha = 1f;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(_timeRemaining / 60);
            int seconds = Mathf.FloorToInt(_timeRemaining % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText)
        {
            scoreText.text = $"Fixed: {_score}/10";
        }
    }

    public bool IsGameActive() => _gameActive;
}