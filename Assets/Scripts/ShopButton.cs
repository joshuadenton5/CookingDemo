using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour //for each shop button
{
    public bool clicked;
    private Text text;
    private Shop shop;
    public double cost;

    void Start()
    {
        text = GetComponentInChildren<Text>();
        shop = FindObjectOfType<Shop>();
        text.text += ": " + cost.ToString("£0.00");
        shop.AddButton(this);
    }

    public double GetCost()
    {
        return cost;
    }

    public void OnClick()
    {
        SetClicked(true);
        shop.OnButtonClick();
    }

    public string NewMealName()
    {
        return RemoveExcess(text.text);
    }

    string RemoveExcess(string str) //removing end of string so meal can be added to menu
    {
        int value = int.MaxValue;
        string s = null;
        for(int i = 0; i < str.Length; i++)
        {
            if (str[i] == ':')
                value = i;
            if (i >= value)
                s += str[i];
        }
        if (str.Contains(s))
            str = str.Replace(s, "");    
        return str;
    }

    public void SetClicked(bool _clicked)
    {
        clicked = _clicked;
    }

    public bool GetClicked()
    {
        return clicked;
    }
}
