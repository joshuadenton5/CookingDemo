using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MealTicket //stores infomation about a meal that is ordered
{
    private string mealText;
    private List<string> mealFood = new List<string>();
    private OrderScreen screen;
    private float ticketTime;
    public bool pauseTimer = false;

    public MealTicket(string text, List<string> food)
    {
        mealText = text;
        foreach (string s in food)
            mealFood.Add(s);
    }

    public IEnumerator TickerTimer(Text t) //timing how long the ticket has been displayed for the player to see
    {
        while(!pauseTimer)
        {
            ticketTime += Time.deltaTime;
            int seconds = (int)(ticketTime % 60);
            int minutes = (int)(ticketTime / 60) % 60;
            string time = string.Format("{0:00}:{1:00}", minutes, seconds);
            t.text = time;
            if (pauseTimer)
            {
                break;
            }
            yield return null;
        }
    }

    public float GetTicketTime()
    {
        return ticketTime;
    }

    public void SetScreen(OrderScreen order)
    {
        screen = order;
    }

    public OrderScreen GetScreen() //linked to order screen
    {
        return screen;
    }

    public List<string> TheFoodInOrder() //the food
    {
        return mealFood;
    }

    public string GetMealName() //meal name
    {
        return mealText;
    }
}
