using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public interface IInteract
{
    void OnClick(RaycastHit hit, Transform t);   
    GameObject GetObject();
    string GetName();
}

public interface IOtherInput
{
    void OnKeyInput(); //for objects that have multiple input (food)
    void OnMouseInput();
}

public interface IPhysics
{
    void DisablePhysics(Transform t);
    void EnablePhysics(Transform t);
    PlaceMarker GetMarker();
    void SetMarker(PlaceMarker m);
}

public interface IInterface
{
    void ChangeInterface(Vector3 pos);
}

public class Interact : MonoBehaviour
{
    private Transform guide;
    public GameObject debris;
    public Text itemText;
    public Image retical;
    public Image downArrow, upArrow, redX;

    Collider current;
    Collider prev;
    private IInteract theObj;
    private IInteract lookAt;
    private IPhysics physics;
    private IInterface ui;
    private AudioManager audioManager;
    private float timeToMove;
    private bool isMoving = false, newColider = false;
    public static bool allowInput = true;

    void Start()
    {
        guide = transform.Find("Guide");
        itemText.text = "";
        retical.color = Color.white;
        timeToMove = .5f;
        audioManager = FindObjectOfType<AudioManager>();
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        downArrow.color = Color.green;
        upArrow.color = Color.green;
        redX.color = Color.red;
    }

    public IInteract GetHolding()
    {
        return theObj;
    }

    public void SetInteract(IInteract o)
    {
        theObj = o;
    }

    public void SetCollider(bool col)
    {
        newColider = col;
    }

    public void SetUI(int index, string text) //controls whats on the interact ui
    {
        switch (index)
        {
            case 1:
                redX.gameObject.SetActive(false);
                downArrow.gameObject.SetActive(true);
                retical.gameObject.SetActive(false);
                upArrow.gameObject.SetActive(false);//when object is in hand and can put down
                itemText.text = text;
                break;
            case 2:
                redX.gameObject.SetActive(true);
                downArrow.gameObject.SetActive(false);
                upArrow.gameObject.SetActive(false); //cannot put down/pick up
                retical.gameObject.SetActive(false);
                itemText.text = text;
                break;
            case 3:
                redX.gameObject.SetActive(false);
                downArrow.gameObject.SetActive(false);
                upArrow.gameObject.SetActive(false); //idle/ no item in ray
                retical.gameObject.SetActive(true);
                itemText.text = text;
                break;
            case 4:
                redX.gameObject.SetActive(false);
                downArrow.gameObject.SetActive(false);
                upArrow.gameObject.SetActive(true); //when can pick up
                retical.gameObject.SetActive(false);
                itemText.text = text;
                break;          
        }
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f) && allowInput) //raycast to detect item
        {
            current = hit.collider;
            ui = hit.collider.GetComponent<IInterface>();
            if (ui != null)
            {
                if (!newColider)
                {
                    ui.ChangeInterface(hit.point); //keeping the ui change from updating every frame
                    prev = hit.collider;
                    newColider = true;
                }

                if (prev != current)
                    newColider = false;
            }
            else
            {
                SetUI(3, "");
                newColider = false;
            }
            if (!isMoving)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    lookAt = hit.collider.GetComponent<IInteract>();
                    if(lookAt != null)
                        lookAt.OnClick(hit, guide); //calling the OnClick function for that object
                }
                else if (Input.GetKeyDown(KeyCode.E)) 
                {
                    IOtherInput other = hit.collider.GetComponent<IOtherInput>();
                    if (other != null)
                        other.OnKeyInput(); //only flips food at the moment
                }
                else if (Input.GetMouseButtonDown(2)) //middle mouse button
                {
                    IOtherInput other = hit.collider.GetComponent<IOtherInput>();
                    if (other != null)
                        other.OnMouseInput(); //dismantles food stack
                }
            }
            else
            {
                SetUI(3, "");
                newColider = false;
            }
        }
        else
        {
            SetUI(3, "");
            newColider = false;
        }

        if (Input.GetMouseButtonDown(1) && allowInput)//
        {
            if (GetHolding() != null && !isMoving)
                OnDrop(hit);
        }
        if (!allowInput)
        {
            itemText.enabled = false;
            retical.enabled = false;
        }
        else
        {
            itemText.enabled = true;
            retical.enabled = true;
        }
    }

    void OnDrop(RaycastHit hit)
    {
        SetPhysics(null);
        GameObject obj = GetHolding().GetObject();
        newColider = false;
        if(obj.GetComponent<Move_S>() != null) //board/plate/tray dropped
        {
            if (obj.CompareTag("Plate")) //checking if its a plate
            {
                if (Physics.Raycast(guide.position, -Vector3.up, out hit, 5f))
                {
                    float dist = Vector3.Distance(guide.position, hit.point);
                    StartCoroutine(BreakStuff(dist, obj));
                }
            }
            Move_S m = GetHolding().GetObject().GetComponent<Move_S>();
            if(m.ListOfFood().Count > 0) //checking if theres food on the object
            {
                foreach (Food_O f in m.ListOfFood()) //resetting varibales for each food object
                {
                    f.GetComponent<IPhysics>().GetMarker().SetOccupation(false);
                    f.SetOnSurface(false);
                    f.ChangeDrag(3);
                    f.firstOnStack = null;
                    f.SetMarker(null);
                    StartCoroutine(CheckObjectCount.CheckPrefabCount(f.gameObject));
                    GameManager.dropCount++;
                }
                m.ListOfFood().Clear();
            }
            m.MarkerList().Clear();
            m.AddMarkerChildren();
        }
        else if (obj.GetComponent<Food_O>() != null) //if food dropped
        {
            Food_O food = obj.GetComponent<Food_O>();
            StartCoroutine(CheckObjectCount.CheckPrefabCount(food.gameObject));
            if(food.foodStack.Count > 0)
            {
                foreach(Food_O f in food.foodStack) //resetting varibales 
                {
                    f.firstOnStack = null;
                    StartCoroutine(CheckObjectCount.CheckPrefabCount(f.gameObject));
                    GameManager.dropCount++;
                    f.GetMarker().SetOccupation(false);
                    f.SetMarker(null);
                }
                food.foodStack.Clear();
            }
        }
        SetInteract(null);
    }

    IEnumerator BreakStuff(float distance, GameObject theObj) //for when a plate is dropped
    {
        theObj.GetComponent<Collider>().isTrigger = true;
        yield return new WaitForSeconds(Mathf.Sqrt(distance / -(Physics.gravity.y / 2)));
        theObj.GetComponent<Collider>().isTrigger = false;
        for (int i = 0; i < 5; i++)
        {
            GameObject d = Instantiate(debris, theObj.transform.position, Quaternion.identity);
            StartCoroutine(CheckObjectCount.CheckPrefabCount(d));
        }
        GameManager.dropCount++;
        Destroy(theObj);
    }

    void SetPhysics(Transform t)
    {
        IPhysics[] phys = GetHolding().GetObject().GetComponentsInChildren<IPhysics>();
        if (phys != null)
            foreach (IPhysics p in phys)
                p.EnablePhysics(t);
    }

    public bool Moving()
    {
        return isMoving;
    }

    public void OnPickUp(Transform t) //when the player picks up an object
    {
        if (!isMoving)
        {
            audioManager.PlaySound("Whosh");
            SetInteract(t.GetComponent<IInteract>());
            GameObject obj = GetHolding().GetObject();
            physics = obj.GetComponent<IPhysics>();
            physics.DisablePhysics(guide);
            StartCoroutine(PickUp(t, timeToMove));
            if (physics.GetMarker() != null)
                physics.GetMarker().SetOccupation(false);
            lookAt = null;
        }
    }

    public void OnPutDown(PlaceMarker toPos, Transform t, bool cook) //when the player places an object down
    {
        if (!isMoving)
        {
            GetHolding().GetObject().transform.parent = null;
            physics = GetHolding().GetObject().GetComponent<IPhysics>();
            physics.SetMarker(toPos);
            StartCoroutine(MoveAndRotate(GetHolding().GetObject().transform, toPos.transform.position, .2f, cook));
            StartCoroutine(WaitForFinish(physics, t, .21f));
            toPos.SetOccupation(true);
            SetUI(3, "");
        }       
    }
  
    IEnumerator WaitForFinish(IPhysics phy, Transform t, float time) //waiting until the put down coroutine is over before applying physics properties
    {
        yield return new WaitForSeconds(time);
        GameObject theObj = GetHolding().GetObject();
        if (theObj.CompareTag("Food"))
            FoodPhysicsProperties(t);
        else if (theObj.CompareTag("Tray") || theObj.CompareTag("Plate") || theObj.CompareTag("Surface"))
            SurfaceFood(t);
        else
            phy.EnablePhysics(t);

        SetInteract(null);
    }
    public IEnumerator PickUp(Transform fromPos, float dur) //the pick up coroutine, saves usuing the animator
    {       
        isMoving = true;
        float counter = 0;
        Vector3 startPos = fromPos.position;
        while (counter < dur)
        {
            counter += Time.deltaTime;
            fromPos.position = Vector3.Lerp(startPos, guide.position, counter / dur); //guide - so the object will always end up in the same position
            yield return null;
        }
        yield return new WaitForSeconds(.01f);
        isMoving = false;
        newColider = false;
    }
    public PlaceMarker GetMarker(GameObject obj)
    {
        PlaceMarker m = obj.GetComponentInChildren<PlaceMarker>();
        if(m != null)
        {
            if (!m.GetOccupation()) return m;
        }
        return null;
    }

    public IEnumerator MoveAndRotate(Transform fromPos, Vector3 toPos, float dur, bool cook) //the put down coroutine, the cook bool depends on whether we want the object to have a deafault rotation 
    {
        isMoving = true;
        float counter = 0;
        Vector3 startPos = fromPos.position;
        Quaternion q = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        if (cook)
            q = Quaternion.identity;
        while (counter < dur)
        {       
            counter += Time.deltaTime;
            fromPos.position = Vector3.Lerp(startPos, toPos, counter / dur);
            fromPos.rotation = Quaternion.Slerp(fromPos.rotation,q,counter / dur);
            yield return null;
        }
        yield return new WaitForSeconds(.3f);
        newColider = false;
        isMoving = false;
    }

    void SurfaceFood(Transform t) //making sure all the food and the object behaves correctly
    {
        Move_S s = GetHolding().GetObject().GetComponent<Move_S>();
        List<Food_O> foodList = s.ListOfFood();
        if(foodList.Count > 0)
        {
            List<Food_O> children = new List<Food_O>();
            for (int i = 0; i < foodList.Count; i++) 
            {
                if (foodList[i].HasChild())
                {
                    //need to link child with parent 
                    foodList[i].GetChild().EnablePhysics(foodList[i].transform); //setting transform to parent food
                    children.Add(foodList[i].GetChild());
                }
                if (!children.Contains(foodList[i]))
                    foodList[i].EnablePhysics(s.transform);
            }
        }       
        s.EnablePhysics(t);
    }

    void FoodPhysicsProperties(Transform t) //for when a only food has been put down 
    {
        Food_O[] foods = GetHolding().GetObject().GetComponentsInChildren<Food_O>();
        if (foods != null)
        {
            for (int i = 0; i < foods.Length; i++)
            {
                if (foods[i] == GetHolding().GetObject().GetComponent<Food_O>())
                    foods[i].EnablePhysics(t);
                else
                    foods[i].EnablePhysics(foods[i - 1].transform);
            }
        }
    }

    public PlaceMarker AnyMarker(List<PlaceMarker> markers, Vector3 pos) //fucntion to find any free placemarker
    {
        if(markers.Count > 0)
        {
            PlaceMarker closest = null;
            float minDistance = float.MaxValue;
            List<float> distances = new List<float>();
            for (int i = 0; i < markers.Count; i++)
            {
                float distance = Vector3.Distance(pos, markers[i].transform.position);
                distances.Add(distance);
                distances[i] = distance;
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    closest = markers[i];
                }
            }
            markers.Remove(closest);
            return closest;
        }
        return null;
    }

    public PlaceMarker ClosestMarker(PlaceMarker[] t, Vector3 pos) //function to find closest marker to where ray hit
    {
        PlaceMarker closest = null;
        float minDistance = float.MaxValue;
        List<float> distances = new List<float>();
        for (int i = 0; i < t.Length; i++)
        {
            float distance = Vector3.Distance(pos, t[i].transform.position);
            distances.Add(distance);
            distances[i] = distance;
            if (distances[i] < minDistance)
            {
                minDistance = distances[i];
                closest = t[i];
            }
        }

        return closest;
    }
}
