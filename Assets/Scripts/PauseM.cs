using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseM : MonoBehaviour //the pause menu
{
    public GameObject pausePanel;
    private FirstPersonCamera cam;
    private PlayerController player;
    private GameManager gameManager;
    private AudioManager audioM;
    private PractiseManager pm;
    private Book book;
    private bool bookP, endP, tutorial, paused;
    public GameObject endDayPanel, bookPanel, tutPanel, infoPanel, timeT, moneyT, controlsP;

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<FirstPersonCamera>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        gameManager = GetComponent<GameManager>();
        audioM = FindObjectOfType<AudioManager>();
        book = FindObjectOfType<Book>();
        pausePanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    public bool OnTutorial()
    {
        return tutorial;
    }

    public static void HideCursor() //static so can be used anywhere
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume() //remume button
    {
        Time.timeScale = 1;
        paused = false;
        if (!endP || !bookP)
        {
            Interact.allowInput = true;
            cam.AllowMove(true);
            player.AllowMove(true);
            HideCursor();
        }

        if (bookP) //checking if any panels are active
        {
            book.bookPanel.SetActive(true);
            bookP = false;
            cam.AllowMove(false);
            player.AllowMove(false);
            ShowCursor();
            Interact.allowInput = false;

        }
        if (endP)
        {
            gameManager.endOfDayPanel.SetActive(true);
            endP = false;
            cam.AllowMove(false);
            player.AllowMove(false);
            ShowCursor();
            Interact.allowInput = false;

        }
        if (tutorial)
        {
            tutPanel.SetActive(true);
            tutorial = false;
        }
        timeT.SetActive(true);
        moneyT.SetActive(true);
        pausePanel.SetActive(false);
        audioM.UnPauseAll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //change to escape
        {
            if (paused)
            {
                if (controlsP.activeInHierarchy)
                    Back();
                else if (infoPanel.activeInHierarchy)
                    Back();
                else
                    Resume();
            }
            else if (!pausePanel.activeInHierarchy)
            {
                Pause();
                paused = true;
            }
                            
        }
    }

    public void MainMenu() //to the main menu
    {
        player.AllowMove(true);
        cam.AllowMove(true);
        SceneManager.LoadScene("_Start");
        Time.timeScale = 1;
    }

    public void Controls() //controls screen
    {
        controlsP.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void Information() //info screen
    {
        infoPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void Back() //back to pause menu
    {
        pausePanel.SetActive(true);
        if (infoPanel.activeInHierarchy)
            infoPanel.SetActive(false);
        else if (controlsP.activeInHierarchy)
            controlsP.SetActive(false);
    }

    public void Pause() //when the player presses esp
    {
        if(endDayPanel!= null)
            if (endDayPanel.activeInHierarchy)
            {
                endDayPanel.SetActive(false);
                endP = true;
            }
        if(bookPanel != null)
            if (bookPanel.activeInHierarchy)
            {
                bookPanel.SetActive(false);
                bookP = true;
            }
        if(tutPanel != null)
            if (tutPanel.activeInHierarchy)
            {
                tutPanel.SetActive(false);
                tutorial = true;
            }

        if (!GameManager.loading)
        {
            Time.timeScale = 0;
            Interact.allowInput = false;
            cam.AllowMove(false);
            player.AllowMove(false);
            pausePanel.SetActive(true);
            timeT.SetActive(false);
            moneyT.SetActive(false);
            ShowCursor();
        }
        audioM.PauseAll();
    }
}
