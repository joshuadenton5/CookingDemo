using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MealAssessment //class to instansiate when a meal needs assessing, was originally in gameManager 
{
    private List<Food_O> food;
    private MealTicket _ticket;
    private string text;
    private Menu men;
    private double money;
    private int mealsSent;
    private string[] responses = new string[7];

    public MealAssessment(List<Food_O> preppedMeal, MealTicket ticket, Menu menu, int mealsThrough) //constuctor 
    {
        food = preppedMeal;
        _ticket = ticket;
        men = menu;
        mealsSent = mealsThrough;
    }

    public string AssessmentText()
    {
        return text;
    }

    public double Money()
    {
        return money;
    }

    bool CheckIfCook(string foodName)
    {       
        List<string> food = _ticket.TheFoodInOrder();
        foreach (string s in food)
        {
            if (s.Contains(foodName))
                if (s.Contains(" -c"))
                    return true;
        }
        return false;
    }

    public void AssessMeal() //probably need splitting up, but accesses meal
    {
        double mealPrice = men.GetMealPrice(_ticket.GetMealName());      
        bool noMoneyBack = false;

        int badPoints = 0;//plus and minus system, needs refining 
        int goodPoints = 0;

        text = "#" + (mealsSent + 1).ToString() + " " + _ticket.GetMealName() + "\n\n"; 

        List<string> foodNames = new List<string>();
        foreach(Food_O f in food)
        {            
            foodNames.Add(f.GetName());
            bool needCook = CheckIfCook(f.GetName()); //checking if need cooking 
            if (f.FoodCooked() && needCook)
            {
                int factor = f.GetCookFactor(); //checking how cooked the food is 
                switch (factor)
                {
                    case -1:
                        text += f.GetName() + " is Undercooked \n";
                        badPoints++;
                        break;
                    case 0:
                        text += f.GetName() + " is Perfectly Cooked \n";
                        goodPoints++;
                        break;
                    case 1:
                        text += f.GetName() + " is Overcooked \n";
                        badPoints++;
                        break;
                    case 2:
                        text += f.GetName() + " is Burnt \n";
                        badPoints += 2;
                        break;
                }
            }
            else if (f.FoodCooked() && !needCook)
            {
                text += f.GetName() + " did not need cooking \n ";
                badPoints++;
            }
            else if (!f.FoodCooked() && needCook)
            {
                text += f.GetName() + " is Raw \n ";
                noMoneyBack = true;
            }                
        }

        for(int i = 0; i < _ticket.TheFoodInOrder().Count; i++)
        {
            if (_ticket.TheFoodInOrder()[i].Contains(" -c"))
            {
                _ticket.TheFoodInOrder()[i] = _ticket.TheFoodInOrder()[i].Replace(" -c", "");
            }
        }

        bool goodLook = CheckPresenation(foodNames, _ticket.TheFoodInOrder(), food);

        Dictionary<string, int> fromPlate = CheckAndSortFood(foodNames); //
        Dictionary<string, int> fromMenu = CheckAndSortFood(_ticket.TheFoodInOrder());

        foreach (KeyValuePair<string, int> entry in fromPlate)
        {
            if (!fromMenu.ContainsKey(entry.Key))
            {
                text += "Wrong food " + entry.Key + "\n";
                badPoints++;
            }
        }

        foreach (KeyValuePair<string, int> entry in fromMenu)
        {
            if (fromPlate.Contains(entry))
            {
                text += "Correct food and amount " + entry + "\n";
                goodPoints++; //4
            }
            if (fromPlate.ContainsKey(entry.Key) && !fromPlate.ContainsValue(entry.Value))
            {
                int plateVal = 0;
                fromPlate.TryGetValue(entry.Key, out plateVal);
                int differece = entry.Value - plateVal;
                if (differece < 0)
                {
                    differece *= -1;
                    text += "There are " + differece + " to many " + entry.Key + "'s" + "\n";
                    badPoints++;
                }
                else
                {
                    text += "You needed " + differece + " more " + entry.Key + "\n";
                    badPoints++;
                }
            }
            else if (!fromPlate.Contains(entry))
            {
                text += "Meal is missing " + entry.Value + " " + entry.Key + "\n";
                badPoints += 2;
                //no money back
            }
        }

        float time = _ticket.GetTicketTime();

        bool customerLeft = false;
        if(time > 180)
            customerLeft = true;

        if (customerLeft)
        {
            money = 0;
            text += CustomerResponses(4, time);
            customerLeft = false;
        }
        else if (noMoneyBack)
        {
            money = 0;
            text += CustomerResponses(0, time);
            noMoneyBack = false;
        }       
        else if (badPoints == 0)
        {
            if (!goodLook)
            {
                money = System.Math.Round(mealPrice / 1.25, 2);
                text += CustomerResponses(5, time);
            }
            else
            {
                money = mealPrice + (mealPrice / System.Math.Round(Random.Range(5f, 15f), 2));
                text += CustomerResponses(1, time);
            }
        }
        else if (badPoints > goodPoints)
        {
            money = 0;
            text += CustomerResponses(0, time);
        }
        else if (goodPoints > badPoints)
        {
            if (!goodLook)
            {
                money = System.Math.Round(mealPrice / 2, 2);
                text += CustomerResponses(6, time);
            }
            else
            {
                money = mealPrice;
                text += CustomerResponses(3, time);
            }           
        }
        else if (goodPoints == badPoints)
        {
            money = System.Math.Round(mealPrice / 2, 2);
            text = CustomerResponses(2, time);
        }
        food.Clear();
        _ticket.TheFoodInOrder().Clear();
        _ticket.GetScreen().SetUse(false);
        _ticket.GetScreen().GetText().text = "Order #" + _ticket.GetScreen().screenNo.ToString();
    }

    Dictionary<string, int> CheckAndSortFood(List<string> listToCheck)
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();
        int i = 0;
        listToCheck.Sort(); //vital 
        while (i != listToCheck.Count)
        {
            string s = listToCheck[i];
            List<string> results = listToCheck.FindAll(x => x.Equals(s)); //finding duplicates 
            int amount = results.Count;
            dic.Add(s, amount); // adding to dic to comapre to 
            i += amount; //incrementing by amount found 
        }
        return dic;
    }

    string CustomerResponses(int index, double _time) //customer response depends on the outcome of the meal, comparing it to the actual meal, what should be 
    {
        responses[0] = "The customer is unhappy with the food and walked off without paying ";
        responses[1] = "The customer is impressed and added a little tip ";
        responses[2] = "The customer is unhappy and only paid half the price of the meal ";
        responses[3] = "The customer is happy ";
        responses[4] = "The customer has been waiting too long and has left the resturant ";
        responses[5] = "The customer questioned the presenation but said the food tasted great ";
        responses[6] = "The customer noted that the meal didn't any where near resemble a " + _ticket.GetMealName() + " and only paid half";
        int seconds = (int)(_time % 60);
        int minutes = (int)(_time / 60) % 60;
        string t = string.Format("{0:00}:{1:00}",minutes,seconds);
        return responses[index] + "\nMoney Received: " + money.ToString("£0.00") + "\nMeal Time: " + t;
    }

    bool CheckPresenation(List<string> playerFood, List<string> actualFood, List<Food_O> onPlate) //basic presenation check 
    {
        if (playerFood.Count == actualFood.Count)
        {
            int last = playerFood.Count - 1;
            if (playerFood[0] == actualFood[0] && playerFood[last] == actualFood[last] && onPlate[last].GetFlipped())
                return true;
        }
        return false;
    }
}
