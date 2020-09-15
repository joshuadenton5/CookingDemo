using UnityEngine;
using System.Linq;
using System.Collections;

public class Cooker : MonoBehaviour,IInteract, IInterface
{
    private Interact i;
    private PlaceMarker[] markers;
    private Color colour;
    private GameManager gameManager;

    void Start()
    {
        i = FindObjectOfType<Interact>();
        markers = GetComponentsInChildren<PlaceMarker>();
        gameManager = FindObjectOfType<GameManager>();
    }
    public string GetName()
    {
        return name;
    }

    public GameObject GetObject()
    {
        return gameObject;
    }

    IEnumerator WaitToCook(Food_O food)
    {
        yield return new WaitForSeconds(.2f);
        food.StartCooking();
    }

    public void OnClick(RaycastHit hit, Transform t)
    {
        if (CheckForFood())
        {
            PlaceMarker m = i.AnyMarker(markers.ToList(), hit.point);
            if (!m.GetOccupation())
            {
                GameObject obj = i.GetHolding().GetObject();
                Food_O food = obj.GetComponent<Food_O>();
                if (!food.HasChild() && CheckScene())
                {
                    StartCoroutine(WaitToCook(food));
                    i.OnPutDown(m, null, true);
                    obj.transform.rotation = Quaternion.identity;
                }
            }            
        }
    }

    bool CheckScene() //checking the scene to see if the food can go on the griddle 
    {
        if (gameManager != null)
            return gameManager.OpenKitchen();
        return true;
    }

    bool CheckForFood()
    {
        if (i.GetHolding() != null)
        {
            GameObject obj = i.GetHolding().GetObject();
            if (obj.CompareTag("Food"))
                return true;
            else
                i.SetUI(2, "");
        }
        return false;
    }

    public void ChangeInterface(Vector3 pos) 
    {
        if (CheckForFood())
        {
            Food_O f = i.GetHolding().GetObject().GetComponent<Food_O>();
            if (f.HasChild())
                i.SetUI(2, "");
            else if (!CheckScene())
                i.SetUI(2, GetName() + " is warming up");
            else
                i.SetUI(1, "Cook " + f.GetName());
        }
        if(i.GetHolding() == null)
        {
            i.SetUI(3, "");
        }
    }
}
