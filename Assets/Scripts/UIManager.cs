using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    public GameObject startpanel;
    public GameObject gamepanel;
    public GameObject gameoverpanel;
    public GameObject winPanel;
    public bool isGameOver = false;
    public GameObject wall;
    public TMP_Text levelText;
    public TMP_Text scoreText;
    public GameObject shooter;

    public void Update()
    {
        levelText.text = "Level :" + DifficultyManager.Instance.currentLevel;
    }
    private void Awake()
    {
        instance = this;
    }
    public void startgame()
    {
        startpanel.SetActive(false);
        gamepanel.SetActive(true);
        Instantiate(wall);
        Instantiate(shooter);
        GridManager.instance.GenerateGrid();
        UpdateScore(Shooter.Instance.score);

        Shooter.Instance.EnableShooting();   
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        startpanel.SetActive(false);
        gamepanel.SetActive(false);
        gameoverpanel.SetActive(true);
    }
    public void WinGame()
    {
        startpanel.SetActive(false);
        winPanel.SetActive(true);
        Shooter.Instance.DisableShooting();
      
    }
    public void NextLevel()
    {
        DifficultyManager.Instance.currentLevel++;
        winPanel.SetActive(false);
        gamepanel.SetActive(true);
        GridManager.instance.GenerateGrid();
        Shooter.Instance.EnableShooting();
    }
    public void UpdateScore(int newScore)
    {
        scoreText.text = "Score : " + newScore.ToString();
    }

}
