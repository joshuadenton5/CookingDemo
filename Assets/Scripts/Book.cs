using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Book : MonoBehaviour, IInteract, IInterface //the log book gameobj
{
    public GameObject bookPanel;
    public Text dayText, bookData;
    private GameManager gameManager;
    private PlayerController pc;
    private FirstPersonCamera fpc;
    private Menu theMenu;
    public Button endShiftB, next, prev, menu;
    private List<string> assessments;
    private List<List<string>> onTheMenu;
    private int bookIndex;
    private bool onMenu;
    private Text butText;
    private int mealNo;
    private Interact i;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        fpc = FindObjectOfType<FirstPersonCamera>();
        pc = FindObjectOfType<PlayerController>();
        theMenu = FindObjectOfType<Menu>();
        bookPanel.SetActive(false);
        butText = menu.GetComponentInChildren<Text>();
        i = FindObjectOfType<Interact>();
    }

    public string GetName()
    {
        name = "Log Book";
        return name;
    }

    public GameObject GetObject()
    {
        return gameObject;
    }

    public void OnClick(RaycastHit hit, Transform t) //bringing up the book panel 
    {
        Interact.allowInput = false; //stopping input from interact script
        onMenu = false;
        bookPanel.SetActive(true);
        gameManager.assessmentPanel.SetActive(false);
        pc.AllowMove(false); //stopping the player from moving 
        fpc.AllowMove(false);
        PauseM.ShowCursor();
        dayText.text = "Feedback";
        butText.text = "Menu";
        if (gameManager.MealTickets().Count == 0 && gameManager.dayTime < 0)//checking if the player can end the shift
            endShiftB.interactable = true;
        else
            endShiftB.interactable = false;

        StartCoroutine(DaysMeals());  //showing feed back if any - default
    }   

    public void Next() //next button
    {
        if (!onMenu) //checking if on menu
        {
            bookIndex++;
            bookData.text = assessments[bookIndex];
            bookData.color = new Color(bookData.color.r, bookData.color.g, bookData.color.b, 0);
            StartCoroutine(FadeIn(bookData.text));
            if (bookIndex == assessments.Count - 1)
                next.interactable = false;
            prev.interactable = true;
        }
        else
        {
            bookIndex++;
            mealNo++;
            StartCoroutine(NextMeal(bookIndex, onTheMenu));
            if (bookIndex == onTheMenu.Count - 1)
                next.interactable = false;
            prev.interactable = true;
        }
    }

    public void Previous() //previous button
    {
        if (!onMenu)
        {
            bookIndex--;
            bookData.text = assessments[bookIndex];
            bookData.color = new Color(bookData.color.r, bookData.color.g, bookData.color.b, 0);
            StartCoroutine(FadeIn(bookData.text));
            if (bookIndex == 0)
                prev.interactable = false;
            next.interactable = true;
        }
        else //onMenu
        {
            bookIndex--;
            mealNo--;
            StartCoroutine(NextMeal(bookIndex, onTheMenu));
            if (bookIndex == 0)
                prev.interactable = false;
            next.interactable = true;
                
        }
    }

    IEnumerator FadeIn(string s)
    {
        bookData.text = s;
        yield return StartCoroutine(GameManager.FadeIn(.5f, bookData));
    }

    IEnumerator DaysMeals() //showing the feedback
    {
        bookIndex = 0;
        bookData.fontSize = 40;
        assessments = new List<string>();
        foreach(string t in gameManager.Assessments())
        {
            assessments.Add(t);
        }
        bookData.color = new Color(bookData.color.r, bookData.color.g, bookData.color.b, 0);
        prev.interactable = false;
        if (assessments.Count > 0)
        {           
            if (assessments.Count > 1)
                next.interactable = true;
            else
                next.interactable = false;
            bookData.text = assessments[bookIndex];
            yield return StartCoroutine(FadeIn(bookData.text));
        }
        else
        {
            string s = "No feedback to show\n";
            next.interactable = false;
            yield return StartCoroutine(FadeIn(s));
        }
    }

    public void Return() //back to game
    {
        Interact.allowInput = true;
        bookPanel.SetActive(false);
        fpc.AllowMove(true);
        pc.AllowMove(true);
        Interact.allowInput = true;
        PauseM.HideCursor();
    }

    IEnumerator NextMeal(int index, List<List<string>> meals) //showing whats on the menu for reference
    {
        bookData.text = "";
        bookData.color = new Color(bookData.color.r, bookData.color.g, bookData.color.b, 0);
        Dictionary<string, int> dic = gameManager.GetDuplicates(meals[index]);   
        for (int i = 0; i < dic.Count; i++)
        {
            string x = null;
            string dash = "";
            KeyValuePair<string, int> element = dic.ElementAt(i);
            if (i != 0)
            {
                if (element.Value == 1)
                    x = "";
                else
                    x = " x " + element.Value;
                dash = "- ";
                bookData.text += dash + element.Key + x + "\n";
            }
            else
            {
                string _name = element.Key;
                _name += theMenu.GetMealPrice(element.Key).ToString(" - £0.00");
                bookData.text += _name+"\n";
            }
            yield return null;
        }
        yield return FadeIn(bookData.text);
    }


    IEnumerator Menu(int index)
    {
        bookData.fontSize = 60;
        bookIndex = 0;
        mealNo = 1;
        prev.interactable = false;
        onTheMenu = gameManager.AvailableMeals();
        if (onTheMenu.Count > 0)
        {
            if (onTheMenu.Count > 1)
                next.interactable = true;
            else
                next.interactable = false;

            yield return NextMeal(bookIndex, onTheMenu);              
        }
    }

    public void ShowMenu() //changing the button text accordingly
    {
        bookData.text = "";
        if (butText.text == "Menu")
        {
            onMenu = true;
            dayText.text = "Menu";
            butText.text = "Feedback";
            StartCoroutine(Menu(bookIndex));
        }
        else if(butText.text == "Feedback")
        {
            dayText.text = "Feedback";
            butText.text = "Menu";
            onMenu = false;
            bookData.text = "";
            StartCoroutine(DaysMeals());
        }
    }

    public void EndShift() //ending the shift
    {
        StartCoroutine(gameManager.EndOfShift());
        bookPanel.SetActive(false);
    }

    public void ChangeInterface(Vector3 pos)
    {
        i.SetUI(1, GetName());
    }
}
