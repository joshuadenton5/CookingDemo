using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Food_O
{
    public GameObject cookingEgg;
    private Transform friedEgg;
    Food_O fEgg;

    public override void Awake()
    {
        base.Awake();
        friedEgg = transform.Find("Fried Egg");
        fEgg = friedEgg.GetComponent<Food_O>();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void StartCooking()
    {
        StartCoroutine("WaitAFrame");
        friedEgg.SetParent(null);
        canvas.transform.SetParent(friedEgg);
        fEgg.SetMarker(GetMarker());
    }

    IEnumerator WaitAFrame()
    { 
        fEgg.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
        fEgg.StartCooking();
        Destroy(gameObject);
    }
}
