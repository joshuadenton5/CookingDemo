using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Move_S : MonoBehaviour,IInteract, IPhysics, IInterface //for trays, borads and plates 
{
    private Interact interact;
    [SerializeField]
    private List<PlaceMarker> avaliableMarkers = new List<PlaceMarker>();
    private Rigidbody rb;
    private Collider col;
    public PlaceMarker marker;
    private readonly List<Food_O> listOfFood = new List<Food_O>();
    public bool startedWithFood = false;
    private bool canInteract = true;
    public GameObject foodType;
    private PlaceMarker[] children;
    private int limit = 27;

    void Awake()
    {
        interact = FindObjectOfType<Interact>();
        children = GetComponentsInChildren<PlaceMarker>();
        AddMarkerChildren();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if(marker != null)
        {
            marker.SetOccupation(true);
            transform.position = marker.transform.position;
        }

        if (foodType != null)
        {
            startedWithFood = true;           
            StartCoroutine(RestockFood(foodType, 0f));
        }
    }

    public void SetInteract(bool set)
    {
        canInteract = set;
    }

    public bool CheckLimit()
    {
        if (listOfFood.Count <= limit)
            return true;
        return false;
    }

    public void AddMarkerChildren()
    {
        foreach (PlaceMarker p in children)
            MarkerList().Add(p);
    }  

    public IEnumerator RestockFood(GameObject foodType, float time)
    {
        List<PlaceMarker> markers = GetComponentsInChildren<PlaceMarker>().ToList();
        
        for(int i = 0; i < markers.Count; i++)
        {
            if (markers[i].transform.parent.CompareTag("Food"))
                continue;

            if (!markers[i].GetOccupation())
            {
                GameObject food = Instantiate(foodType, markers[i].transform.position, markers[i].transform.rotation);
                Food_O f = food.GetComponent<Food_O>();
                f.SetMarker(markers[i]);
                f.EnablePhysics(transform);
                f.ChangeDrag(f.defaultDrag);
                f.SetOnSurface(true);
                f.SetFreshness(true);
                markers[i].SetOccupation(true);
                ListOfFood().Add(f);
                MarkerList().Remove(markers[i]);
                yield return new WaitForSeconds(time);
            }       
        }
    }

    public List<PlaceMarker> MarkerList()
    {
        return avaliableMarkers;
    }

    public void CheckForFood() //checking if this object has food on it
    {
        List<Food_O> children = new List<Food_O>();
        if (ListOfFood().Count > 0)
        {             
            for(int i = 0; i < ListOfFood().Count; i++)
            {
                if(ListOfFood()[i].HasChild())
                {
                    ListOfFood()[i].GetChild().DisablePhysics(ListOfFood()[i].transform);
                    children.Add(ListOfFood()[i].GetChild());
                }
                if(!children.Contains(ListOfFood()[i]))
                {
                    ListOfFood()[i].DisablePhysics(transform);
                }
            }           
        }
    }
    
    void CheckPlateCount() //checking if plates need to be restocked 
    {        
        PlateStock parent = GetComponentInParent<PlateStock>();
        if(parent != null)
        {
            if (parent.PlateList().Contains(this))
                parent.PlateList().Remove(this);

            if (parent.PlateList().Count == 0)
                StartCoroutine(parent.RestockShelf(parent.plate, .2f));
        }                   
    }

    public void OnClick(RaycastHit hit, Transform t)
    {
        IInteract obj = interact.GetHolding();
        if (obj == null && !CookUI.isMoving && canInteract)
        {           
            if (!gameObject.CompareTag("Tray")) //trays cannot be picked up, after some deliberartion 
            {
                if (gameObject.CompareTag("Plate"))
                    CheckPlateCount();
                interact.OnPickUp(transform);
                CheckForFood();
            }            
        }
        else if(obj != null && !CookUI.isMoving && canInteract)
        {
            if (obj.GetObject().CompareTag("Food") && !interact.Moving()) //putiing food on move_s
            {
                if(foodType != null) //for trays 
                {
                    if(foodType.name == obj.GetName())
                        PutFoodOn(hit, obj.GetObject());
                }
                else
                    PutFoodOn(hit, obj.GetObject()); //doesn't need to distinguish between food types
            }
        }
    }

    void PutFoodOn(RaycastHit hit, GameObject food) //putting food on 
    {
        PlaceMarker a = interact.AnyMarker(MarkerList(), hit.point);
        if (a != null)
        {
            Food_O[] foods = food.GetComponentsInChildren<Food_O>();
            if (foods != null) //checking how many objects
            {
                int i = 1;
                foreach (Food_O f in foods)
                {
                    ListOfFood().Add(f); //setting the food varibales
                    f.SetOnSurface(true);
                    f.ChangeDrag(f.defaultDrag);
                    f.stack = i;
                    i++;
                }
            }
            interact.OnPutDown(a, transform, false);
        }
    }

    public List<Food_O> ListOfFood()
    {
        return listOfFood;
    }

    public void DisablePhysics(Transform t)
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        col.isTrigger = true;
        transform.parent = t;
        if(t!=null)
            transform.rotation = t.rotation;
        if (GetComponentsInChildren<Collider>() != null)
            foreach (Collider c in gameObject.GetComponentsInChildren<Collider>())
                c.isTrigger = true;
    }

    public void EnablePhysics(Transform t)
    {
        col.isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.parent = t;
    }

    public PlaceMarker GetMarker()
    {
        return marker;
    }

    public void SetMarker(PlaceMarker m)
    {
        marker = m;
    }

    public GameObject GetObject()
    {
        return gameObject;
    }   

    public string GetName()
    {
        if (name.Contains("Clone)"))
        {
            name = name.Replace("(Clone)", "");
        }
        return name;
    }

    public void ChangeInterface(Vector3 pos)
    {
        if (interact.GetHolding() != null && canInteract)
        {
            GameObject obj = interact.GetHolding().GetObject();
            if (obj.CompareTag("Food"))
            {
                if (avaliableMarkers.Count > 0)
                    interact.SetUI(1, GetName());
                else
                    interact.SetUI(2, "");
            }
            else
                interact.SetUI(2, "");
        }
        else if(interact.GetHolding() == null && canInteract)
        {
            if (!gameObject.CompareTag("Tray"))
                interact.SetUI(4, GetName());
            else
                interact.SetUI(3, "");
        }
    }
}
