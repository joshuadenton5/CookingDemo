using System.Collections.Generic;
using UnityEngine;

public class SendArea : MonoBehaviour, IInteract, IInterface //where the player puts a plate to send to customer
{
    private PlaceMarker[] markers;
    private Interact i;
    private GameManager gameManager;
    public PractiseManager practise;
    public int tableNum;
    public string mealName;
    public List<string> foodForSend; //debugging 
    private MealTicket ticket;

    void Start()
    {
        markers = GetComponentsInChildren<PlaceMarker>();
        i = FindObjectOfType<Interact>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnClick(RaycastHit hit, Transform t)
    {
        IInteract obj = i.GetHolding();
        if(obj != null)
        {
            if (obj.GetObject().CompareTag("Plate") && ticket != null)
            {
                PlaceMarker m = i.ClosestMarker(markers, hit.point);
                if (!m.GetOccupation())
                {
                    SendMeal();
                    SetTicket(null);
                    i.OnPutDown(m, null, true);
                }                   
            }
        }       
    }

    void SendMeal()
    {
        Move_S plateS = i.GetHolding().GetObject().GetComponent<Move_S>();       
        if (gameManager != null)
            gameManager.SendMeal(plateS.ListOfFood(), ticket, plateS);
        else
            practise.SendMeal(plateS.ListOfFood(), ticket, plateS);
    }

    public void SetTicket(MealTicket _ticket) //storing ticket information in the area
    {
        ticket = _ticket;
        if (ticket != null)
        {
            foodForSend = ticket.TheFoodInOrder();
            //mealName = ticket.GetMealName();
        }      
    }

    public GameObject GetObject()
    {
        return null;
    }

    public string GetName()
    {
        return name;
    }
   
    public void ChangeInterface(Vector3 pos)
    {
        if (i.GetHolding() != null)
        {
            GameObject obj = i.GetHolding().GetObject();
            if (obj.CompareTag("Plate"))
            {
                if (ticket != null)
                    i.SetUI(1, "Send To Table " + tableNum);
                else
                    i.SetUI(1, "This Table is Unoccupied");
            }
            else
                i.SetUI(2, "");
        }
        else
            i.SetUI(3, "");
    }
}
