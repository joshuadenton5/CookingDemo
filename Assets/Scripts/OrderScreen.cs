using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderScreen : MonoBehaviour //currently three of these, display a meal 
{
    private Text[] displayText;
    private bool inUse;
    public int screenNo;
    private float timer;

    void Start()
    {
        displayText = GetComponentsInChildren<Text>();
        displayText[1].text = "00:00";
    }

    public bool GetUse()
    {
        return inUse;
    }

    public void SetUse(bool use)
    {
        inUse = use;
    }

    public Text GetText()
    {
        return displayText[0];
    }

    public Text TimeText()
    {
        return displayText[1];
    }

    public void StartTimer(IEnumerator t) 
    {
        StartCoroutine(t);
    }
}
