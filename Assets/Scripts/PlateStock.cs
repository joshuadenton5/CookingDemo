using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateStock : MonoBehaviour
{
    public GameObject plate;
    List<Move_S> plates = new List<Move_S>();
    List<PlaceMarker> plateMarkers = new List<PlaceMarker>();

    void Start()
    {
        Stationary_S sibling = GetComponent<Stationary_S>();
        foreach (PlaceMarker p in sibling.PlateMarkers())
            plateMarkers.Add(p);
    }

    public List<Move_S> PlateList()
    {
        return plates;
    }

    public IEnumerator RestockShelf(GameObject type, float time)
    {
        if (plate != null)
        {
            for (int i = 0; i < plateMarkers.Count; i++)
            {
                GameObject p = Instantiate(plate, plateMarkers[i].transform.position, Quaternion.identity);
                Move_S surface = p.GetComponentInChildren<Move_S>();
                surface.SetMarker(plateMarkers[i]);
                plateMarkers[i].SetOccupation(true);
                plates.Add(surface);
                p.transform.SetParent(transform);
                yield return new WaitForSeconds(time);
            }
        }
    }
}
