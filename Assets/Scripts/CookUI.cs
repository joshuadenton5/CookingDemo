using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookUI 
{
    public bool isPaused;
    public bool startedCook;
    public float angle = 180;
    public static bool isMoving;
    public bool offCooker;
    public float currentFoodT;
    float overCookTime;
    float foodCookTime;

    public int CurrentCookTime()
    {
        int s = int.MaxValue;
        if (currentFoodT < 0 - foodCookTime / 5f)
            s = -1; //undercooked
        else if (currentFoodT < foodCookTime / 5f)
            s = 0; //perfect
        else if (currentFoodT > foodCookTime / 2.5f)
            s = 2; //burnt
        else if (currentFoodT > foodCookTime / 5f)
            s = 1; //overcooked
      
        return s;
    }

    public float CurrentFoodTime()
    {
        return currentFoodT;
    }

    public IEnumerator Cook(float cookTime, Image cookIm) //when the food goes onto the griddle
    {
        overCookTime = cookTime / 2;
        currentFoodT = -cookTime;
        foodCookTime = cookTime;
        startedCook = true;
        float count = 0;
        Transform t = cookIm.transform.parent.parent;
        MeshRenderer mesh = t.GetComponentInChildren<MeshRenderer>();
        Material[] mats = mesh.materials;
        Color[] newColour = new Color[mats.Length];
        for (int i = 0; i < mats.Length; i++) //changing the objects colour while the food cooks
        {
            newColour[i] = mats[i].color * 0.25f;
            newColour[i].a = 1;
        }
        while (currentFoodT <= 0)
        {
            while (!isPaused) //if the food hasn't been taken off the griddle
            {
                currentFoodT += Time.deltaTime;
                count += Time.deltaTime;
                cookIm.fillAmount = count / cookTime;
                for(int i = 0; i < mats.Length; i++)
                {
                    mats[i].color = Color.Lerp(mats[i].color, newColour[i], Time.deltaTime / cookTime);
                }
                if (cookIm.fillAmount >= 1)
                    break;
                yield return null;
            }
            yield return null;
        }
        cookIm.color = Color.red; //changing the colour of the bar to red to let the player know food is overcooking 
        cookIm.fillAmount = 0;
        count = 0;        
        while (currentFoodT <= overCookTime)
        {
            while (!isPaused)
            {
                count += Time.deltaTime;
                currentFoodT += Time.deltaTime;
                cookIm.fillAmount = count / overCookTime;
                for(int i = 0; i < mats.Length; i++)
                {
                    mats[i].color = Color.Lerp(mats[i].color, Color.black, Time.deltaTime / overCookTime); //food will turn black if left on too long
                }
                if (cookIm.fillAmount >= 1)
                    break;
                yield return null;
            }            
            yield return null;
        }
        //food.CreateSmoke(food.transform.position); //saved for later
    }

    public IEnumerator FlipFood(Transform main, Transform mdel, Vector3 toPos, float dur, Food_O food) //when the player flips a food object
    {
        isMoving = true;
        food.ChangeDrag(0);
        float counter = 0;
        angle += 180;
        float a = 0;
        food.DisablePhysics(food.transform);
        Vector3 startPos = main.position;
        isMoving = true;
        while (counter < dur) //moving up 
        {
            counter += Time.deltaTime;
            main.position = Vector3.Lerp(startPos, toPos, counter / dur);
            yield return null;
        }
        while(a < 180) //then rotating 
        {
            mdel.Rotate(Vector3.forward, 10);
            a += 10;
            yield return null;
        }
        food.EnablePhysics(food.transform); //allowing gravity to take it down
        float dist = .1f;
        float cal = Mathf.Sqrt(dist / -(Physics.gravity.y / 2));
        yield return new WaitForSeconds(cal); //only changing the drag after the food has finished falling
        food.ChangeDrag(3); 
        isMoving = false;
    }
}
