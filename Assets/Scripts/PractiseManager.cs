using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PractiseManager : MonoBehaviour //manager for tutorial 
{
    public Text moneyText, moneyFade, assementText, loadtText, breakDownText;
    public Button next, prev, end;
    private Menu menu;
    public GameObject assessmentPanel, loadPanel, tutPanel;
    private List<MealTicket> mealTickets = new List<MealTicket>();
    private List<string> mealAssesments = new List<string>();
    private List<List<string>> currentMeals = new List<List<string>>();
    private PauseM pause;

    private double currentCash = 5;
    private bool endOfDay, running, nextDay, enter;
    public static bool loading;
    public int round = 1;
    private int mealsThrough = 0;
    private double daysEarnings, dayMoneySpent;

    private PlayerController player;
    private FirstPersonCamera cam;
    private AudioManager audioManager;
    private CallForWaiter waiter;
    [SerializeField]
    private SendArea[] sendAreas;
    [SerializeField]
    private OrderScreen[] screens;
    public Camera mainCam;
    public Transform defaultPos;
    string[] texts;
    int textIndex;
    bool over;
    int arrayLenth = 14;
    bool paused = false;
    bool show;

    void Start()
    {
        menu = GetComponent<Menu>();
        waiter = GetComponent<CallForWaiter>();
        audioManager = GetComponent<AudioManager>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        pause = GetComponent<PauseM>();
        player.transform.position = defaultPos.position;
        player.transform.rotation = defaultPos.rotation;
        cam = GameObject.FindWithTag("MainCamera").GetComponent<FirstPersonCamera>();
        assessmentPanel.SetActive(false);
        loadPanel.SetActive(false);
        assementText = assessmentPanel.GetComponentInChildren<Text>();
        moneyText.text = currentCash.ToString("£0.00");
        AddToCurrent(GetMealIndex("Classic Burger"));
        tutPanel.SetActive(false);
        texts = new string[arrayLenth];
        StartCoroutine("Loading");
    }

    IEnumerator BreakDown() //runs until the player presses return - displays some of the games mechanics
    {
        tutPanel.SetActive(true);
        over = false;
        show = true;
        while (!over)
        {
            bool right = Input.GetKeyDown(KeyCode.RightArrow);
            bool left = Input.GetKeyDown(KeyCode.LeftArrow);
            bool enter = Input.GetKeyDown(KeyCode.Return);
            if (!paused)
            {
                if (right && !pause.OnTutorial())
                    Next();
                else if (left && !pause.OnTutorial())
                    Previous();
            }          
            
            if (enter)
                over = true;

            if (show)
            {
                breakDownText.text = ReturnInstruction(textIndex);
                show = false;
            }
            yield return null;
        }
        tutPanel.SetActive(false);
    }

    public void End()
    {
        tutPanel.SetActive(false);
        over = true;
    }

    public void Next() //next insruction, right arrow key
    {
        if (textIndex == arrayLenth - 1)
            textIndex = arrayLenth - 1;
        else
        {
            textIndex++;
            show = true;
        }
    }

    public void Previous()//previous text, left arrow key 
    {
        if (textIndex == 0)
            textIndex = 0;
        else
        {
            textIndex--;
            show = true;
        }
    }

    string ReturnInstruction(int index) //instructions for the tutuorial
    {
        texts[0] = "Use this area to get used to the game mechanics.\n\nUse the left and right arrow keys to cycle through instructions.\nAlterativley, press the return key to skip this tutorial.";
        texts[1] = "Great, use the WASD keys to move while using the mouse to look around the Kitchen.";
        texts[2] = "The order screen on the wall shows any meal tickets that need to be prepped and cooked.\n\nIn this case, someone has ordered a Classic Burger, which requires a patty, chopped onion and 2 buns.\n The '-c' next to the patty and chopped onion means they need to be cooked.";
        texts[3] = "Many objects in the game can be picked up by clicking on them, showing a green arrow with the objects name below it.\nOnce an object is in hand, you can click where the green arrow appears to put it down or click the RMB to drop the item.";
        texts[4] = "The Griddle, on your left as you are looking at the doors, can be used to cook food.\n\nThe bar shows you how long it takes to cook the item.\n\nThe bar will turn red if you have left it on the griddle for too long!";
        texts[5] = "Once all the food is cooked, you can begin creating the meal.\n\nTo stack food, simply click on a FOOD object while holding a FOOD object.\nThis will join the food together so when you pick it up next time, all food in the stack will be in hand.";
        texts[6] = "If you wish to dismantle the food stack, press the MIDDLE MOUSE BUTTON while looking at the food stack.";
        texts[7] = "You can press the E key to flip a FOOD object 180 degrees. For the burger, it might be worth flipping one of the buns.";
        texts[8] = "Once you think the meal is ready, grab a plate (lower shelf) and pop the meal on.\n\nHead over to the table by the doors and put the plate on the green sqaure.";
        texts[9] = "Here, you will see a pop up displaying what the customer made of your meal. The amount of money recicved from the meal depends on how well you did.\nIf you take too long, the customer will leave.";
        texts[10] = "In the game, you will be charged every time you pick up a fresh food object, which are located in the fridge and on the shelves, so watch your funds!";
        texts[11] = "The game will also feature three order screens along with three tables to send food too.";
        texts[12] = "A key object in the game, the LOGBOOK (not in this tutorial yet!), contains all the feedback from all of the meals sent per day. You can also check what is on the menu and end your shift here. More information on this can be found in the pause menu.";
        texts[13] = "If you would like more practise, click on the icon located on the table at the back of the kitchen. This will bring in a new order\n\nPress enter to hide this text or use the pause menu to return to the main menu.";
        return texts[index];
    }

    public int GetMealIndex(string str)
    {
        List<List<string>> listOfList = menu.AllMealsAndFood();
        int index = int.MaxValue;
        for (int i = 0; i < listOfList.Count; i++)
        {
            string s = listOfList[i][0];
            if (s == (str))
                index = i;
        }
        if (index == int.MaxValue)
            Debug.LogError("Invalid Item");
        //Debug.Log(str);
        return index;
    }

    public void AddToCurrent(int index)
    {
        currentMeals.Add(menu.AllMealsAndFood()[index]);
    }

    IEnumerator Loading()
    {
        loading = true;
        loadPanel.SetActive(true);
        player.AllowMove(false);
        cam.AllowMove(false);
        Interact.allowInput = false;
        PauseM.HideCursor();
        mainCam.enabled = true;
        loadtText.text = "Loading";
        float t = UnityEngine.Random.Range(2, 5);
        mainCam.transform.rotation = Quaternion.identity;
        string dot = ".";
        while (t > 0)
        {
            t -= 1;
            loadtText.text += dot;
            yield return new WaitForSeconds(1f);
        }
        loadPanel.SetActive(false);
        yield return new WaitForSeconds(1);
        player.AllowMove(true);
        cam.AllowMove(true);
        Interact.allowInput = true;
        CheckAvailability();
        StartCoroutine(BreakDown());
    }

    public IEnumerator UpdateMoney(double amount, double incre)
    {
        StartCoroutine(FadeText(1f, moneyFade, amount, ""));
        double total = currentCash + amount;
        while (Math.Round(currentCash, 2) != Math.Round(total, 2))
        {
            currentCash += incre;
            moneyText.text = currentCash.ToString("£0.00");
            if (currentCash <= 0)
            {
                currentCash = 0;
                moneyText.text = currentCash.ToString("£0.00");
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ShowResults(MealAssessment assessment)
    {
        while (running)
            yield return null;
        assementText.text = assessment.AssessmentText();
        running = true;
        assessmentPanel.SetActive(true);
        yield return new WaitForSeconds(8);
        assessmentPanel.SetActive(false);
        running = false;
    }

    public bool GetItemPrice(string _name)
    {
        double price = menu.GetMealPrice(_name);
        if (currentCash - price >= 0)
        {
            StartCoroutine(UpdateMoney(-price, -0.05));
            dayMoneySpent += price;
            return true;
        }
        StartCoroutine(FlashText(moneyText));
        return false;
    }
    List<string> TheFood(List<string> mealAndFood)
    {
        List<string> food = new List<string>();
        for (int i = 0; i < mealAndFood.Count; i++)
        {
            if (i != 0)
                food.Add(mealAndFood[i]);
        }
        return food;
    }
    public IEnumerator FlashText(Text t)
    {
        t.color = Color.red;
        yield return new WaitForSeconds(.1f);
        t.color = Color.white;
    }
    List<string> RandomMeal()
    {
        int i = UnityEngine.Random.Range(0, currentMeals.Count);
        return currentMeals[i];
    }
    public void CreateTicket(OrderScreen screen)
    {
        List<string> obj = RandomMeal();
        MealTicket ticket = new MealTicket(obj[0], TheFood(obj)); //storing information in seperate class
        ticket.SetScreen(screen);
        //StartCoroutine(ticket.TickerTimer(screen.TimeText()));
        mealTickets.Add(ticket);
        DisplayMeal(ticket.TheFoodInOrder(), ticket.GetMealName(), screen.GetText());
        sendAreas[(screen.screenNo) - 1].SetTicket(ticket);
        screen.SetUse(true);
        audioManager.PlaySound("Bell");
    }
    public void CheckAvailability()
    {
        foreach (OrderScreen screen in screens)
        {
            if (!screen.GetUse())
            {
                CreateTicket(screen);
                break;
            }
        }
    }

    public Dictionary<string, int> GetDuplicates(List<string> contents)
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (string item in contents)
        {
            if (!dic.ContainsKey(item))
                dic.Add(item, 1);
            else
            {
                int count = 0;
                dic.TryGetValue(item, out count);
                dic.Remove(item);
                dic.Add(item, count + 1);
            }
        }
        return dic;
    }

    void DisplayMeal(List<string> food, string meal, Text textToDisplay)
    {
        meal += "\n";
        string add = null;
        for (int i = 0; i < 6; i++)
        {
            add += "- ";
        }
        meal += add + "";
        foreach (KeyValuePair<string, int> entry in GetDuplicates(food))
        {
            string str = null;
            if (entry.Value == 1)
                str = "";
            else
                str = " x " + entry.Value;

            meal += "\n" + entry.Key + str;
        }

        textToDisplay.text += "\n";
        textToDisplay.text += meal;
    }

    IEnumerator FadeText(float dur, Text text, double value, string str)
    {
        text.text = str;
        string s = null;
        if (value != 0)
        {
            if (Mathf.Sign((float)value) == -1)
                s = "";
            else
                s = "+";
            text.text = s + value.ToString("F2");
        }
        text.color = new Color(text.color.r, text.color.g, text.color.r, 1f);
        while (text.color.a > 0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.r, text.color.a - Time.deltaTime / dur);
            yield return null;
        }
    }

    public void SendMeal(List<Food_O> food, MealTicket ticket, Move_S plate)
    {
        MealAssessment assessment = new MealAssessment(food, ticket, menu, mealsThrough);
        ticket.pauseTimer = true;
        assessment.AssessMeal();
        double moneyBack = assessment.Money();
        StartCoroutine(UpdateMoney(moneyBack, moneyBack / 10d));
        daysEarnings += moneyBack;
        int mealIndex = mealTickets.IndexOf(ticket);
        mealTickets.Remove(mealTickets[mealIndex]);
        mealsThrough++;
        StartCoroutine(ShowResults(assessment));
        StartCoroutine(waiter.GetWaiter(sendAreas[ticket.GetScreen().screenNo - 1].transform, plate));
        mealAssesments.Add(assessment.AssessmentText());
    }
}
