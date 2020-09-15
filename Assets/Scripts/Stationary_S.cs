using System.Collections.Generic;
using UnityEngine;

public class Stationary_S : MonoBehaviour,IInteract, IInterface //script for non moveable obejcts like tables
{
    private PlaceMarker[] markers;
    private Interact i;
    private List<PlaceMarker> pMarkers = new List<PlaceMarker>();
    void Start()
    {
        if (gameObject.CompareTag("PShelf")) //checking if this object will have plates
            PlatesOnly();
        else
            markers = GetComponentsInChildren<PlaceMarker>();

        i = FindObjectOfType<Interact>();
    }

    private void PlatesOnly()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform g in children)
        {
            if (g.CompareTag("Marker"))
                pMarkers.Add(g.GetComponent<PlaceMarker>());
        }
    }

    public List<PlaceMarker> PlateMarkers()
    {
        return pMarkers;
    }

    public void OnClick(RaycastHit hit, Transform t)
    {
        IInteract inter = i.GetHolding();
        if (inter != null)
        {
            PlaceMarker m = null;

            if (gameObject.CompareTag("PShelf"))
                 m = i.ClosestMarker(pMarkers.ToArray(), hit.point);
            else
                m= i.ClosestMarker(markers, hit.point);

            if (!m.GetOccupation())
            {
                if (inter.GetObject().CompareTag("Food"))
                    PutFoodDown(inter.GetObject());

                i.OnPutDown(m, null, false);
            }
        }
    }

    void PutFoodDown(GameObject theObj)
    {
        Food_O[] foods = theObj.GetComponentsInChildren<Food_O>();
        foreach(Food_O f in foods)
            f.ChangeDrag(f.defaultDrag);
    }

    public GameObject GetObject()
    {
        return gameObject;
    }

    public string GetName()
    {
        return name;
    }

    bool CheckEmptyMarker(Vector3 pos)
    {
        foreach (PlaceMarker p in markers)
            if (!p.GetOccupation())
                return true;
        PlaceMarker pl = i.ClosestMarker(markers, pos);
        if (!pl.GetOccupation())
            return true;
        return false;

    }

    bool CheckList()
    {
        if (pMarkers.Count == 0)
            return true;
        return false;
    }

    public void ChangeInterface(Vector3 pos)
    {
        if (i.GetHolding() != null)
        {
            if (CompareTag("PShelf"))
                if (CheckList())
                    i.SetUI(1, GetName());
                else
                    i.SetUI(3, "");
            else
                if (CheckEmptyMarker(pos))
                i.SetUI(1, GetName());
            else
                i.SetUI(3, "");

        }
        else
            i.SetUI(3, "");
    }
}
