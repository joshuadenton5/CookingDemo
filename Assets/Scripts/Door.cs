using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour,IInteract, IInterface
{
    private bool doorOpen;
    float defaultAngle;
    public float angle;
    private Transform hinge;
    private string text;
    private Color color;
    Interact i;
    bool moving;
    private AudioManager au;

    void Start()
    {
        hinge = transform.Find("Hinge");
        defaultAngle = angle;
        i = FindObjectOfType<Interact>();
        au = FindObjectOfType<AudioManager>();
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
        angle = -angle;
        StartCoroutine(OpenDoor(Vector3.up, hinge, angle, .4f));
    }
        
    IEnumerator OpenDoor(Vector3 axis, Transform t, float angle, float dur)
    {
        moving = true;
        float elapsed = 0;
        float rotated = 0;
        i.SetUI(3, "");
        if (angle == defaultAngle)
            au.PlaySound("DoorClose");
        else
            au.PlaySound("DoorOpen");
        while (elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            transform.RotateAround(t.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        transform.RotateAround(t.position, axis, angle - rotated);
        
       // else

            moving = false;
        i.SetCollider(false);
    }

    public void ChangeInterface(Vector3 pos)
    {
        if (angle == defaultAngle && !moving)
        {
            i.SetUI(1, "Open Fridge");
        }
        else if(!moving)
            i.SetUI(1, "Close Fridge");
    }
}
