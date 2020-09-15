using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour //class for the shop where player can buy meals
{
    GameManager gameManager;
    public GameObject shopPanel;

    [SerializeField]
    private List<ShopButton> shopButtons;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        shopPanel.SetActive(false);
    }

    public void ShowShop()
    {
        gameManager.endOfDayPanel.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void ExitShop() //back to stats 
    {
        gameManager.endOfDayPanel.SetActive(true);
        shopPanel.SetActive(false);
    } 

    public void OnButtonClick() //checking which button has been clicked 
    {
        foreach(ShopButton b in shopButtons)
        {
            if (b.GetClicked())
            {
                CheckBalance(b);
                b.SetClicked(false);
            }
        }       
    }

    public void AddButton(ShopButton button)
    {
        shopButtons.Add(button);
    }

    void CheckBalance(ShopButton button) //checking if the player can affod the item
    {
        if (gameManager.Money() >= button.GetCost())
        {
            int index = gameManager.GetMealIndex(button.NewMealName());
            gameManager.AddToCurrent(index);
            StartCoroutine(gameManager.UpdateMoney(-button.GetCost(), -.5));
            Debug.Log("Meal added to menu!!");
            button.GetComponent<Button>().interactable = false;
        }
        else
        {
            Debug.Log("Not enough Money!!");
            StartCoroutine(gameManager.FlashText(gameManager.moneyText));
        }
    }
}
