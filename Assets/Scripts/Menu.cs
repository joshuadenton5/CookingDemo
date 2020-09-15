using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour //class to find and get variables for food, meals, cooktimes and prices 
{
    private string[] readText;
    private string[] myFiles;
    private string textLine;
    private List<string> food = new List<string>();
    private List<List<string>> mealsAndFood = new List<List<string>>();
    private string theMeal;
    private int numberOfLines;
    public TextAsset asset; //text file to read from
    private string[] textList;
    private List<string> trimmedList = new List<string>();

    void Awake()
    {
        textList = asset.text.Split('\n');
        foreach(string s in textList)
        {
            string trim = s;
            trim = trim.Trim(new char[] { '\r', '\n' });//pain, have learned for future
            trimmedList.Add(trim);
        }
        numberOfLines = trimmedList.Count;
        for (int i = 0; i < numberOfLines; i++)
        {
            textLine = trimmedList[i]; //line
            if(textLine[0] == '#')
            {
                textLine = textLine.Replace("#", "");
                List<string> meal = new List<string>
                {
                    textLine
                };
                foreach (string s in GetFoodInMeal(textLine, i))
                {
                    string str = s;
                    meal.Add(str);
                }
                mealsAndFood.Add(meal);
            }
        }
    }

    public List<List<string>> AllMealsAndFood()
    {
        return mealsAndFood;
    }

    List<string> GetFoodInMeal(string t, int i)
    {
        List<string> _food = new List<string>();
        i++;
        t = trimmedList[i];
        while (t[0] != '#' && t[0] != '@') 
        {
            _food.Add(t);
            i++;
            t = trimmedList[i];
        }
        return _food;
    }

    public List<string> GetRandomMeal()
    {
        int i = Random.Range(0, mealsAndFood.Count);
        return mealsAndFood[i];      
    }   

    double GetInt(string t)
    {       
        double val = int.MaxValue;
        string time = "";

        for(int i = 0; i < t.Length; i++)
        {
            if(t[i] == '=')
                val = i;
            if(i > val)
                time += t[i];
        }
        double _time = double.Parse(time); //finding the time limits/cook times
        return _time;
    }

    public int GetFoodIndex(string n)
    {
        int index = int.MaxValue;
        for (int i = 0; i < numberOfLines; i++)
        {
            if (trimmedList[i].Contains(n))
            {
                index = i;
                break;
            }
        }
        if (index == int.MaxValue)
        {
            Debug.LogError("Couldn't Find item " + n);
        }
        return index;
    }

    public string GetFoodName(int index)
    {
        string s = trimmedList[index];
        return s;
    }

    public string FindFood(int index)
    {
        string line = null;
        for(int i = 0; i < numberOfLines; i++)
        {
            if(i == index)
            {
                line = trimmedList[i];
            }
        }
        if (line == null)
            Debug.LogError("Couldn't find item");
        return line;
    }

   
    void GetFood(string t, int i) //getting strings to print
    {
        i++;
        t = trimmedList[i];
        while (t[0] != '#' && t[0] != '@')
        {
            if (t.Contains(" -c"))
                t = t.Replace(" -c", "");
            food.Add(t);          
            i++;
            t = trimmedList[i];
        }
    }

    public double GetMealPrice(string _name)
    {
        int index = GetFoodIndex("$");
        double price = 0;
        for(int i = index+1; i < numberOfLines; i++)
        {
            string t = trimmedList[i];
            string newName = FindDot(t);
            if (newName == _name)
            {
                price = GetInt(t);
            }
        }
        return price;
    }

    string FindDot(string str)
    {
        string o = null;
        for(int i = 0; i < str.Length; i++)
        {
            if (str[i] != '.')
                o += str[i];
            else
                break;
        }
        return o;
    }

    public bool NeedCooking(string name)
    {
        int index = GetFoodIndex(name);
        string t = trimmedList[index];
        if (t.Contains(" -c"))
            return true;

        return false;
    }

    public double GetItemTime(string name)
    {
        int index = GetFoodIndex("CookTimes");
        double _time = 0;
        for(int i = index; i < numberOfLines; i++)
        {
            string t = trimmedList[i];
            if (t.Contains(name))
            {
                _time = GetInt(t);
                break;
            }
        }
        return _time;
    }   
}
