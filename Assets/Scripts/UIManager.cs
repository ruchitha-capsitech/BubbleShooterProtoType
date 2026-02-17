using UnityEngine;
using UnityEngine.UIElements;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    public GameObject startpanel;
    public GameObject gamepanel;
    public GameObject gameoverpanel;
    public bool isGameOver = false;
    public GameObject wall;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startgame()
    {
        startpanel.SetActive(false);
        gamepanel.SetActive(true);
        Instantiate(wall);
        GridManager.instance.GenerateGrid();

    }
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
       
        startpanel.SetActive(false);
        gamepanel.SetActive(false);
        gameoverpanel.SetActive(true);
    }
}
