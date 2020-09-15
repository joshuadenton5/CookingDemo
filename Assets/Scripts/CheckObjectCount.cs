using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObjectCount : MonoBehaviour //checking objects on the floor, still not finished
{
    public static List<GameObject> objects = new List<GameObject>();

    void Start()
    {
        objects.Clear();
    }

    public static IEnumerator CheckPrefabCount(GameObject obj)
    {
        objects.Add(obj);
        if (objects.Count > 25)
        {           
            yield return (Strobe(objects[0]));
        }
    }
    static IEnumerator Strobe(GameObject obj)
    {
        Food_O f = objects[0].GetComponent<Food_O>();
        if (f != null)
            Destroy(f);
        objects.Remove(objects[0]);
        int i = 3;
        while(i > 0)
        {
            i--;
            yield return new WaitForSeconds(.1f);
            obj.SetActive(false);
            yield return new WaitForSeconds(.1f);
            obj.SetActive(true);
        }
        Destroy(obj);
    }
}
