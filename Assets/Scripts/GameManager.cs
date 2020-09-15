using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    //declaring variables
    public Text[] endDayUI; 
    public Text moneyText, moneyFade, countDownText, timer, assementText, load;
    public Button shop, nextDayB;
    private Menu menu;
    public GameObject assessmentPanel, endOfDayPanel, toMainButton, loadPanel;
    private List<MealTicket> mealTickets = new List<MealTicket>();
    private List<string> mealAssesments = new List<string>();
    private List<List<string>> currentMeals = new List<List<string>>();

    public double currentCash;
    private bool endOfDay, running, nextDay, openKitchen;
    public static bool loading;
    public int round = 1;
    private int mealsThrough = 0;
    private double daysEarnings, dayMoneySpent;
    public float dayTime;

    private PlayerController player;
    private FirstPersonCamera cam;
    private AudioManager audioManager;
    private CallForWaiter waiter;
    [SerializeField]
    private SendArea[] sendAreas;
    [SerializeField]
    private OrderScreen[] screens;
    public Camera mainCam;
    public Camera secCam;
    public Transform defaultPos;
    public static float dropCount;

    public GameObject smoke;

    void Start() //setting references and defaults
    {
        menu = GetComponent<Menu>();
        waiter = GetComponent<CallForWaiter>();
        audioManager = GetComponent<AudioManager>();
        toMainButton.SetActive(false);
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        player.transform.position = defaultPos.position;
        player.transform.rotation = defaultPos.rotation;
        secCam.transform.position = defaultPos.position;
        secCam.transform.rotation = Quaternion.identity;
        cam = GameObject.FindWithTag("MainCamera").GetComponent<FirstPersonCamera>();
        assessmentPanel.SetActive(false);
        loadPanel.SetActive(false);
        assementText = assessmentPanel.GetComponentInChildren<Text>();
        moneyText.text = currentCash.ToString("£0.00");
        endOfDayPanel.SetActive(false);
        AddToCurrent(GetMealIndex("Hotdog"));
        AddToCurrent(GetMealIndex("Classic Burger"));
        //AddToCurrent(GetMealIndex("Bacon Cheese Burger"));
        //AddToCurrent(GetMealIndex("Sausage & Egg Bap"));
        //AddToCurrent(GetMealIndex("Sausage, Bacon & Egg Bap"));
        StartCoroutine("Loading");
    }

    public List<string> Assessments()
    {
        return mealAssesments;
    }

    public List<List<string>> AvailableMeals()
    {
        return currentMeals;
    }

    public List<MealTicket> MealTickets()
    {
        return mealTickets;
    }

    public bool OpenKitchen()
    {
        return openKitchen;
    }

    public int GetMealIndex(string str)
    {
        List<List<string>> listOfList = menu.AllMealsAndFood();
        int index = int.MaxValue;        
        for (int i = 0; i < listOfList.Count; i++)
        {
            string s = listOfList[i][0]; //the meal name is always the first element
            if (s == (str))
                index = i;
        }
        if (index == int.MaxValue)
            Debug.LogError("Invalid Item");
        return index;
    }

    public double Money()
    {
        return currentCash;
    }
 
    public void StartShift()
    {
        endOfDay = false;
        shop.interactable = false;
        nextDayB.interactable = false;
        timer.text = 0.ToString("00:00");
        StartCoroutine("ShiftCountDown");
    }

    public IEnumerator ShiftCountDown() //small promt before the kitchen doors open
    {
        float i = 3;
        yield return new WaitForSeconds(1);
        countDownText.text = Mathf.Round(i).ToString();
        while (i >= 0)
        {
            countDownText.text = Mathf.Round(i).ToString();
            i -= Time.deltaTime;
            yield return null;
        }
        CheckAvailability(false);
        nextDay = false;
        StartCoroutine("ShiftTime");
        countDownText.text = "Doors Open!";
        yield return new WaitForSeconds(2);
        countDownText.text = "";
    }

    IEnumerator ShiftTime()
    {
        while (dayTime > 0)
        {
            dayTime -= Time.deltaTime;
            int seconds = (int)(dayTime % 60); //so the time is displayed in minutes and seconds
            int minutes = (int)(dayTime / 60) % 60;
            string time = string.Format("{0:00}:{1:00}", minutes, seconds);
            timer.text = time;
            yield return null;
        }
        endOfDay = true;
        timer.color = Color.red;
        while (mealTickets.Count > 0) //waiting until the timer has hit zero, ie shift time
        {
            yield return null;
        }
        timer.color = Color.white;
        timer.text = "Shift Over. See book";
    }

    List<string> TheFood(List<string> mealAndFood) //retrieving the food only, not the meal name
    {
        List<string> food = new List<string>();
        for(int i = 1; i < mealAndFood.Count; i++)
        {
            food.Add(mealAndFood[i]);
        }
        return food;
    }

    public void AddToCurrent(int index) //adding to current menu
    {
        currentMeals.Add(menu.AllMealsAndFood()[index]);
    }

    List<string> RandomMeal()
    {
        int i = UnityEngine.Random.Range(0, currentMeals.Count);
        return currentMeals[i];
    }

    void CreateTicket(OrderScreen screen)
    {
        List<string> obj = RandomMeal(); //random meal from the menu
        MealTicket ticket = new MealTicket(obj[0], TheFood(obj)); //storing information in seperate class
        ticket.SetScreen(screen);
        StartCoroutine(ticket.TickerTimer(screen.TimeText())); //starting the timer
        mealTickets.Add(ticket); 
        DisplayMeal(ticket.TheFoodInOrder(), ticket.GetMealName(), screen.GetText()); //displaying the meal
        sendAreas[(screen.screenNo) - 1].SetTicket(ticket); //assigning the ticket to the correct send area
        screen.SetUse(true);
        audioManager.PlaySound("Bell"); 
    }

    public void CheckAvailability(bool forced) //checks if theres an avaivable slot for a new meal
    {       
        foreach(OrderScreen screen in screens)
        {
            if (!screen.GetUse())
            {
                CreateTicket(screen);               
                break;
            }
        }
        if (!forced)
        {
            float timeBetweenOrders = UnityEngine.Random.Range(30, 50);
            StartCoroutine(MealTimer(timeBetweenOrders));//time between tickets
        }
    }

    public Dictionary<string, int> GetDuplicates(List<string> contents) //returns a dictionary that checks for duplicates 
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach(string item in contents)
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
        return dic; //this now contains name of item as key and number of that item as value
    }

    void DisplayMeal(List<string> food, string meal, Text textToDisplay) //displaying the meal to the screen
    {            
        meal += "\n";     
        string add = null;
        for(int i = 0; i < 6; i++)
        {
            add += "- ";
        }
        meal += add + "";       
        foreach(KeyValuePair<string, int> entry in GetDuplicates(food))
        {
            string str = null;
            if (entry.Value == 1)
                str = "";
            else
                str = " x " + entry.Value; //saving the duplicates 
            meal += "\n" + entry.Key + str;
        }
        textToDisplay.text += "\n";
        textToDisplay.text += meal;
    }

    IEnumerator MealTimer(float time) //timer between each new order
    {
        while(time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        if (!endOfDay)
            CheckAvailability(false);
        time = 0;
    }

    IEnumerator FadeText(float dur, Text text, double value, string str) //fading the text using alpha value
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

    public IEnumerator UpdateMoney(double amount, double incre) //updating the players money
    {
        StartCoroutine(FadeText(1f, moneyFade, amount, ""));
        double total = currentCash + amount;
        while (Math.Round(currentCash, 2) != Math.Round(total, 2))
        {
            currentCash += incre;
            moneyText.text = currentCash.ToString("£0.00");
            if(currentCash <= 0)
            {
                currentCash = 0;
                moneyText.text = currentCash.ToString("£0.00");
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ShowResults(MealAssessment assessment) //showing the customer feedback for 6 seconds
    {
        while (running)
            yield return null;
        assementText.text = assessment.AssessmentText();
        running = true;
        assessmentPanel.SetActive(true);
        yield return new WaitForSeconds(6);
        assessmentPanel.SetActive(false);
        running = false;
    }

    public bool GetItemPrice(string _name)
    {       
        double price = menu.GetMealPrice(_name);
        if(currentCash - price >= 0) //player has enough credit
        {
            StartCoroutine(UpdateMoney(-price, -0.05));
            dayMoneySpent += price;
            return true;
        }
        StartCoroutine(FlashText(moneyText)); //flashing if the player does not have enough credit
        return false;
    }

    public void SendMeal(List<Food_O> food, MealTicket ticket, Move_S plate) //when the player puts plate on send area
    {
        MealAssessment assessment = new MealAssessment(food, ticket, menu, mealsThrough); //meal assessment 
        ticket.pauseTimer = true;
        assessment.AssessMeal();
        double moneyBack = assessment.Money(); //getting the money value
        StartCoroutine(UpdateMoney(moneyBack, moneyBack/10d)); //updating credit
        daysEarnings += moneyBack;
        int mealIndex = mealTickets.IndexOf(ticket);
        mealTickets.Remove(mealTickets[mealIndex]); //removing from list so new meal can come through
        mealsThrough++;
        StartCoroutine(ShowResults(assessment)); //showing feedback
        StartCoroutine(waiter.GetWaiter(sendAreas[ticket.GetScreen().screenNo -1].transform, plate)); //calling for the waiter
        mealAssesments.Add(assessment.AssessmentText()); 
        DisallowInteraction(plate);
    }

    void DisallowInteraction(Move_S plate) //not allowing the player to touch the plate/food once its been sent
    {
        plate.SetInteract(false);
        Food_O[] foodOnPlate = plate.GetComponentsInChildren<Food_O>();
        if (foodOnPlate != null)
            foreach (Food_O f in foodOnPlate)
                f.SetInterct(false);
    }

    public IEnumerator EndOfShift() //end shift button on the log book
    {
        StartCoroutine(CameraPan(nextDay));
        timer.text = "";
        Interact.allowInput = false; //disabling interaction, resetting lists and varibales for next day
        player.AllowMove(false);
        cam.AllowMove(false);
        PauseM.ShowCursor();
        mealAssesments.Clear();
        endOfDayPanel.SetActive(true);
        double kCost = EndOfDayRestock(true) + Math.Round(daysEarnings /10, 2);
        dayMoneySpent += kCost;
        endDayUI[0].text = "Day " + round.ToString(); //displaying day staistics 
        endDayUI[1].text = "Todays Earnings: " + daysEarnings.ToString("£0.00");
        endDayUI[3].text = "Todays Total Expenditure: " + dayMoneySpent.ToString("£0.00");
        endDayUI[2].text = "Kitchen Clean and Staff Wages: " + kCost.ToString("-£0.00");
        endDayUI[4].text = "Todays Profit: " + (daysEarnings - dayMoneySpent).ToString("£0.00");
        endDayUI[5].text = "Meals Sent: " + mealsThrough.ToString();

        foreach (Text t in endDayUI)
            t.color = new Color(0, 0, 0, 0);

        for(int i = 0; i < endDayUI.Length; i++)
        {
            if (i == 2)
                StartCoroutine(UpdateMoney(-kCost, -.01));
            yield return StartCoroutine(FadeIn(.9f, endDayUI[i]));
            yield return null;
        }
        if(currentCash <= 0) // when the player has no credit
        {          
            foreach(Text t in endDayUI)
            {
                yield return StartCoroutine(FadeOut(.8f, t));
            }
            endDayUI[0].text = "You have gone bankrupt!";
            yield return StartCoroutine(FadeIn(1.2f, endDayUI[0]));
            toMainButton.SetActive(true);
        }
        else
        {
            shop.interactable = true;
            nextDayB.interactable = true;
        }
        dayMoneySpent = 0;
        daysEarnings = 0;
        round++;
        mealsThrough = 0;
    }

    public static IEnumerator FadeIn(float dur, Text t)
    {
        while (t.color.a < 1f)
        {
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a + Time.deltaTime / dur);
            yield return null;
        }
    }

    public IEnumerator FadeOut(float dur, Text t)
    {
        while (t.color.a > 0f)
        {
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a - Time.deltaTime / dur);
            yield return null;
        }
    }

    public IEnumerator FlashText(Text t)
    {
        t.color = Color.red;
        yield return new WaitForSeconds(.1f);
        t.color = Color.white;
    }

    public double EndOfDayRestock(bool clean)
    {
        double cleanCost = UnityEngine.Random.Range(0.45f, .51f);
        Move_S[] allS = FindObjectsOfType<Move_S>();
        Food_O[] allFood = FindObjectsOfType<Food_O>();
        GameObject[] allDeb = GameObject.FindGameObjectsWithTag("debris");
        if (clean)
        {
            foreach (Food_O f in allFood)
            {
                Destroy(f.gameObject);
            }
            foreach (Move_S surface in allS)
            {
                if (surface.GetMarker() != null)
                    surface.GetMarker().SetOccupation(false);
                Destroy(surface.gameObject);
            }

            GameObject[] allMObjs = GameObject.FindGameObjectsWithTag("Marker");
            PlaceMarker[] allMarkers = new PlaceMarker[allMObjs.Length];
            for (int i = 0; i < allMarkers.Length; i++)
                allMarkers[i] = allMObjs[i].GetComponent<PlaceMarker>();

            foreach (PlaceMarker s in allMarkers)
            {
                s.SpwanObject();
            }
            if (allDeb != null)
                foreach (GameObject g in allDeb)
                    Destroy(g);
            cleanCost += dropCount * .2;
            dropCount = 0;
        }       
        return cleanCost;
    }

    public IEnumerator CameraPan(bool condition)
    {
        secCam.enabled = true;
        mainCam.enabled = false;
        while (!condition)
        {
            yield return StartCoroutine(Move(secCam.transform, secCam.transform.position + secCam.transform.forward * 8, 7));
            yield return RotateAnObject(secCam.transform.up, secCam.transform, 90, 3);
            yield return StartCoroutine(Move(secCam.transform, secCam.transform.position + secCam.transform.forward * 3, 2.1f));
            yield return RotateAnObject(secCam.transform.up, secCam.transform, 90, 3);         
        }
    }

    public static IEnumerator Move(Transform fromPos, Vector3 toPos, float dur)
    {
        float i = 0;
        Vector3 startPos = fromPos.position;
        while (i < dur)
        {
            i += Time.deltaTime;
            if (fromPos != null)
            {
                fromPos.position = Vector3.Lerp(startPos, toPos, i / dur);
                yield return null;
            }
        }
    }

    public static IEnumerator RotateAnObject(Vector3 axis, Transform obj, float angle, float dur)
    {
        float elapsed = 0;
        float rotated = 0;
        while (elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            obj.transform.RotateAround(obj.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        obj.transform.RotateAround(obj.position, axis, angle - rotated);
    }

    public void NextDay()
    {
        StopAllCoroutines();
        StartCoroutine(Loading());
        secCam.transform.position = defaultPos.position;
        secCam.transform.rotation = Quaternion.identity;
        nextDay = true;
        Assessments().Clear();
        player.transform.position = defaultPos.position;
        player.transform.rotation = defaultPos.rotation;
        endOfDayPanel.SetActive(false);       
        dayTime = 250;//planning on having different length day times or increasing
    }

    IEnumerator Loading()// the loading screen
    {
        loading = true;
        loadPanel.SetActive(true);
        player.AllowMove(false);
        cam.AllowMove(false);
        Interact.allowInput = false;
        PauseM.HideCursor();
        mainCam.enabled = true;
        secCam.enabled = false;
        load.text = "Loading";
        float t = UnityEngine.Random.Range(2, 5);
        mainCam.transform.rotation = Quaternion.identity;
        string dot = ".";
        while(t > 0)
        {
            t -= 1;
            load.text += dot;
            yield return new WaitForSeconds(1f);
        }
        loadPanel.SetActive(false);
        yield return new WaitForSeconds(.4f);
        player.AllowMove(true);
        cam.AllowMove(true);
        Interact.allowInput = true;
        timer.text = "Press Enter to open the kitcken!";
        loading = false;
        openKitchen = false;
        while (!openKitchen)
        {
            openKitchen = Input.GetKeyDown(KeyCode.Return) | Input.GetKeyDown(KeyCode.KeypadEnter);
            yield return null;
        }
        StartShift();
    }
}
