using UnityEngine;

public class Bell : MonoBehaviour, IInteract, IInterface //for the tutorial, so the player can continue practising 
{
    PractiseManager pm;
    Interact i;

    void Start()
    {
        pm = FindObjectOfType<PractiseManager>();
        i = FindObjectOfType<Interact>();
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
        pm.CheckAvailability();
    }

    public void ChangeInterface(Vector3 pos)
    {
        i.SetUI(1, "Get Order");
    }
}
