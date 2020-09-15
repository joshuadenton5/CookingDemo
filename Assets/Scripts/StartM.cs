using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartM : MonoBehaviour //the main menu 
{
    public GameObject infoPanel, back;
    public GameObject[] buttons;
    private bool play;

    private void Start()
    {
        infoPanel.SetActive(false);
        play = false;
        StartCoroutine("StartCamera");
    }

    public void Play() //play button
    {
        SceneManager.LoadScene("Main");
        Interact.allowInput = true;
        play = true;
    }

    public void Tutorial() //takes to the tutorial
    {
        SceneManager.LoadScene("Practise");
        Interact.allowInput = true;
    }

    public void Information() //info panel
    {
        infoPanel.SetActive(true);
        back.SetActive(true);
        foreach (GameObject button in buttons)
            button.SetActive(false);
    }

    public void Back() //back to the start menu
    {
        infoPanel.SetActive(false);
        back.SetActive(false);
        foreach (GameObject button in buttons)
            button.SetActive(true);
    }

    IEnumerator StartCamera() //movement of the camera 
    {
        float time = 5;
        while (!play)
        {
            yield return StartCoroutine(MoveAndRotate(8, time));
            yield return new WaitForSeconds(time);
            yield return StartCoroutine(MoveAndRotate(3, time * 1.2307f));
            yield return new WaitForSeconds(time * 1.2307f);
        }
    }

    IEnumerator MoveAndRotate(float dist, float time)
    {
        StartCoroutine(GameManager.Move(transform, transform.position + transform.forward * dist, time));
        StartCoroutine(GameManager.RotateAnObject(transform.up, transform, 90, time));
        yield return null;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
