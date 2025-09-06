using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int startingResources = 200;
    public int CurrentResources { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI resourcesText;
    public Slider towerHealthSlider;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    private bool paused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentResources = startingResources;
        UpdateResourcesUI();
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void AddResources(int amount)
    {
        CurrentResources += amount;
        UpdateResourcesUI();
    }

    public void SpendResources(int amount)
    {
        CurrentResources -= amount;
        if (CurrentResources < 0) CurrentResources = 0;
        UpdateResourcesUI();
    }

    void UpdateResourcesUI()
    {
        if (resourcesText) resourcesText.text = CurrentResources.ToString();
    }

    public void UpdateTowerHealth(int current, int max)
    {
        if (towerHealthSlider)
        {
            towerHealthSlider.maxValue = max;
            towerHealthSlider.value = current;
        }
    }

    public void OnGameOver()
    {
        Time.timeScale = 0f;
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (pausePanel) pausePanel.SetActive(paused);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
