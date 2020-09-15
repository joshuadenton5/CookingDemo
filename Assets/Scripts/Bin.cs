using System.Collections;
using UnityEngine;

public class Bin : MonoBehaviour, IInteract, IInterface //for the bin in the game, doesn't have much use at the moment 
{
    private Interact interact;
    private PlaceMarker marker;
    private Transform hinge;
    private Transform lid;
    private AudioManager audioManager;

    void Start()
    {
        marker = GetComponentInChildren<PlaceMarker>();
        interact = FindObjectOfType<Interact>();
        hinge = transform.Find("Hinge");
        lid = transform.Find("Lid");
        audioManager = FindObjectOfType<AudioManager>();
    }

    public string GetName()
    {
        return name;
    }

    public GameObject GetObject()
    {
        return gameObject;
    }

    public void OnClick(RaycastHit hit, Transform t)
    {
        if(interact.GetHolding() != null)
        {
            GameObject current = interact.GetHolding().GetObject();

            if (current.CompareTag("Food"))
            {
                StartCoroutine(BinIt(.3f));
                interact.OnPutDown(marker, transform, false);
                EndFood(current);
            }
        }
    }

    void EndFood(GameObject food)
    {
        Destroy(food, 3);
    }

    IEnumerator BinIt(float dur)
    {
        yield return StartCoroutine(OpenDoor(lid.right,lid, hinge, 90, dur));
        yield return new WaitForSeconds(.05f);
        yield return StartCoroutine(OpenDoor(lid.right, lid, hinge, -90, dur));
        audioManager.PlaySound("Bin");
    }

    IEnumerator OpenDoor(Vector3 axis, Transform obj, Transform t, float angle, float dur)
    {
        float elapsed = 0;
        float rotated = 0;
        while (elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            obj.RotateAround(t.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        obj.RotateAround(t.position, axis, angle - rotated);
    }

    public void ChangeInterface(Vector3 pos)
    {
        if (interact.GetHolding() != null)
        {
            GameObject obj = interact.GetHolding().GetObject();
            if (obj.CompareTag("Food"))
                interact.SetUI(1, "Bin " + interact.GetHolding().GetName());
            else
                interact.SetUI(2, "");
        }
        else
            interact.SetUI(3, "");
    }
}
