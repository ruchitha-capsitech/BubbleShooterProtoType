using UnityEngine;
using UnityEngine.UIElements;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    public GameObject startpanel;
    public GameObject gamepanel;
    public GameObject gameoverpanel;
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
    }
    public void GameOver()
    {
        startpanel.SetActive(false);
        gamepanel.SetActive(false);
        gameoverpanel.SetActive(true);
    }
}
