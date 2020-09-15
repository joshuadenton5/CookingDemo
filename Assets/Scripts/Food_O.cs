using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Food_O : MonoBehaviour, IInteract, IPhysics, IOtherInput, IInterface
{
    private Interact i;
    private Rigidbody rb;
    private Collider col;
    private Menu menu;
    private GameManager manager;
    private AudioManager au;
    public PlaceMarker marker;
    private PlaceMarker thisMarker;
    private string _name;

    [SerializeField]
    private bool onSurface;

    [SerializeField]
    private bool onCooker;

    private bool cooked;
    public float defaultDrag;
    private int foodTime = 0;
    public int stack = 1;
    public CookUI cook = new CookUI();
    private Image cookBar;
    protected Canvas canvas;
    public List<Food_O> foodStack = new List<Food_O>();
    public Food_O firstOnStack;
    private bool fresh, canInteract = true, flipped = false;
    public bool inStack;
   
    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        SetName(name);
    }

    public virtual void Start()
    {
        i = FindObjectOfType<Interact>();
        au = FindObjectOfType<AudioManager>();
        menu = FindObjectOfType<Menu>();
        manager = FindObjectOfType<GameManager>();
        foodTime = (int)menu.GetItemTime(GetName());
        canvas = transform.Find("Canvas").GetComponent<Canvas>();
        cookBar = canvas.transform.Find("CookBar").GetComponent<Image>();
        if (canvas.gameObject.activeInHierarchy)
            canvas.gameObject.SetActive(false);
        thisMarker = GetComponentInChildren<PlaceMarker>();
        defaultDrag = 600;
        stack = 1;
    }

    public void SetOnCooker(bool _onCooker)
    {
        onCooker = _onCooker;
    }
    public bool OnCooker()
    {
        return onCooker;
    }
    public bool OnSurface()
    {
        return onSurface;
    }
    public void SetOnSurface(bool p)
    {
        onSurface = p;
    }
    public bool FoodCooked()
    {
        return cooked;
    }
    public bool GetFlipped()
    {
        return flipped;
    }   
    public bool GetFreshness()
    {
        return fresh;
    }
    public void SetFreshness(bool _fresh)
    {
        fresh = _fresh;
    }

    bool CheckForChildren(Food_O bottom) //checking if the food has children and configuring accordingly
    {
        Food_O[] food = bottom.GetComponentsInChildren<Food_O>();
        Move_S m = bottom.GetComponentInParent<Move_S>(); //if the foo is stacked, bottom is the food at the bottom of the stack
        if (bottom.GetFreshness()) //untouched food
        {
            if (manager != null) //cheap way to configure, checking scene basically
                if (manager.GetItemPrice(bottom.GetName())) //if can afford
                    bottom.SetFreshness(false);
                else
                    return false;
        }
        i.OnPickUp(bottom.transform);
        if (m != null) //checking if on board/ plate / tray
        {
            for (int i = 0; i < food.Length; i++)
            {
                food[i].SetOnSurface(false);
                m.ListOfFood().Remove(food[i]); //removing the food from the script list               
            }
            if (firstOnStack != null || stack == 1) //if theres a stack or singular food
            {
                m.MarkerList().Add(bottom.GetMarker()); //readding the marker to the board/plate/tray for future 
                bottom.SetMarker(null);
            }
            if (m.startedWithFood) //checking if the surface needs replenishing
                if(m.ListOfFood().Count == 0)
                    StartCoroutine(m.RestockFood(m.foodType, .2f));
        }
        for (int i = 0; i < food.Length; i++) //for all food, be it one or more
        {
            food[i].stack = i + 1; //changing the food variables
            food[i].ChangeDrag(3);
            if (CheckObjectCount.objects.Contains(food[i].gameObject))
                CheckObjectCount.objects.Remove(food[i].gameObject);
            if(food[i] != bottom)
                food[i].DisablePhysics(food[i - 1].transform);
            food[i].inStack = false;
        }
        return true;
    }

    public int GetCookFactor()
    {
        return cook.CurrentCookTime();
    }

    void PutFoodOnFood(GameObject obj) //when food is in hand and want to place on other food.
    {
        Food_O[] food = obj.GetComponentsInChildren<Food_O>(); 
        Food_O last = food[food.Length - 1]; //getting the last in the stack, may only be the one
        if (last.stack + stack <= 7 && !inStack) //may change stack limit
        {
            PlaceMarker m = i.GetMarker(gameObject);
            if (m != null)
            {
                for (int i = 0; i < food.Length; i++)
                {
                    food[i].ChangeDrag(food[i].defaultDrag); //restting the drag and stack varibales
                    if (food[i] != obj.GetComponent<Food_O>())
                        food[i].stack = food[i - 1].stack + 1;
                    else
                        food[i].stack = stack + 1;
                }
                if (GetParents() != null)
                {
                    Food_O bottom = GetParents()[GetParents().Length - 1];//getting item at the bottom of the stack
                    foreach (Food_O f in food)
                    {
                        bottom.foodStack.Add(f); //reordering the stack
                        f.firstOnStack = bottom;
                    }
                }
                Move_S move = m.GetComponentInParent<Move_S>(); //checing if the placemarker belongs to a plate/board/tray
                if (move != null)
                {
                    for (int i = 0; i < food.Length; i++)
                    {
                        food[i].SetOnSurface(true);
                        move.ListOfFood().Add(food[i]); //adding food to the surface accordingly
                    }
                }
                i.OnPutDown(m, transform, false);
            }
        }
    }

    public void SetInterct(bool set)
    {
        canInteract = set;
    }

    public virtual void StartCooking() //only virtual for one object
    {
        SetOnCooker(true);
        cook.isPaused = false;
        if (!cook.startedCook)
        {
            StartCoroutine(cook.Cook(foodTime, cookBar));
            cooked = true;
        }
        canvas.gameObject.SetActive(true);
        au.AddFoodSound(gameObject);
    }

    public void CreateSmoke(Vector3 pos)
    {
        Instantiate(manager.smoke, pos, Quaternion.identity);
    } // not implemented yet

    public void OnMouseInput()
    {
        //DismantleStack();
    }
    public void OnKeyInput()
    {
        if (!CookUI.isMoving && foodStack.Count == 0 && firstOnStack == null) 
        {
            Vector3 pos = transform.position + Vector3.up / 10;
            Transform t = transform.GetChild(0);
            StartCoroutine(cook.FlipFood(transform, t, pos, .2f, this));
            flipped = !flipped;
        }
    }

    void DismantleStack() //ugly, needs readjusting, but separates each food object when in a stack
    {
        if(firstOnStack != null)
        {
            if (firstOnStack.foodStack.Count > 0)
            {
                Food_O food = firstOnStack;
                foreach (Food_O f in food.foodStack)
                {
                    if(f.firstOnStack != null)
                        f.transform.SetParent(f.firstOnStack.transform.parent);
                    f.firstOnStack = null;
                    if(f.GetMarker() != null)
                        f.GetMarker().SetOccupation(false);
                    if(f.thisMarker != null)
                        f.thisMarker.SetOccupation(false);
                    f.inStack = true;
                }
                food.foodStack.Clear();
            }
        }        
    }

    public void OnClick(RaycastHit hit, Transform t) //when the player clicks on a food object
    {
        if(i.GetHolding() == null && canInteract) //if theres nothing in hand
        {            
            if (OnCooker() && !CookUI.isMoving) //is cooking
            {
                canvas.gameObject.SetActive(false); //remove cook bar
                cook.isPaused = true; //stip cooking
                SetOnCooker(false);
                i.OnPickUp(transform); //pick up
                au.RemoveSource(GetComponent<AudioSource>()); //stop sound 
                Destroy(GetComponent<AudioSource>());
            }
            else if(!CookUI.isMoving)
            {
                if (firstOnStack != null)
                    CheckForChildren(firstOnStack);
                else //either no stack or bottom
                    CheckForChildren(this);
            }
        }
        else if(i.GetHolding() != null && canInteract)
        {
            GameObject obj = i.GetHolding().GetObject();            
            if(obj.CompareTag("Food") && !i.Moving()) //can only put other food on food
            {
                //if (!OnCooker())
                    //PutFoodOnFood(obj);
            }
        }
    }
    public Food_O[] GetParents()
    {
        Food_O[] parents = GetComponentsInParent<Food_O>();        
        return parents;
    } //getting parents 
    public Food_O GetChild()
    {
        Food_O[] food = GetComponentsInChildren<Food_O>();
        foreach (Food_O f in food)
        {
            if (f != this)
                return f;
        }
        return null;
    } //getting child

    public bool HasChild()
    {
        Food_O[] food = GetComponentsInChildren<Food_O>();        
        foreach(Food_O f in food)
        {
            if (f != this)
                return true;
        }
        return false;
    } //checking for child

    public void ChangeDrag(float val) //changing the drag 
    {
        rb.angularDrag = val;
        rb.drag = val/20;
    }

    public string GetName()
    {
        //removing clone
        if (_name.Contains("(Clone)"))
            _name = _name.Replace("(Clone)", "");
        return _name;
    }    

    public void SetName(string n)
    {
        _name = n;
    }

    public void DisablePhysics(Transform t) //allowing the object to use physics
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.parent = t; //setting parent where necessary
        transform.rotation = t.rotation;

        if (GetComponentsInChildren<Collider>() != null)
        {
            foreach (Collider c in gameObject.GetComponentsInChildren<Collider>())
                c.isTrigger = true;
        }
    }

    public void EnablePhysics(Transform t) //allowing physics
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

    void CheckStack()
    {
        if(thisMarker != null)
        {
            if(!thisMarker.GetOccupation())
                i.SetUI(1, GetName());
            else
                i.SetUI(2, "");
        }
    }

    public void ChangeInterface(Vector3 pos) //method from IUi, changes depending on what the player is looking at/holding 
    {
        if(i.GetHolding() != null && canInteract)
        {
            GameObject obj = i.GetHolding().GetObject();
            if (obj.CompareTag("Food"))
            {
                Food_O[] food = obj.GetComponentsInChildren<Food_O>();
                Food_O last = food[food.Length - 1];
                if (last.stack + stack <= 7)
                    CheckStack();
                else
                    i.SetUI(2, "");
            }
            else
                i.SetUI(2, "");
                
        }
        else if(i.GetHolding() == null && canInteract)
            i.SetUI(4, GetName());
    }
}
