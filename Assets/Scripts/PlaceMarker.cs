using UnityEngine;

public class PlaceMarker : MonoBehaviour //every position the player can put an object
{
    [SerializeField]
    private bool isOccupied; //debugging purposes
    [SerializeField]
    private GameObject surface;

    private void Start()
    {
        SpwanObject();
    }

    public void SpwanObject() //function to spawn an object is required 
    {
        if(surface != null)
        {
            GameObject obj = Instantiate(surface, transform.position, Quaternion.identity);
            Move_S s = obj.GetComponentInChildren<Move_S>();
            if (s != null)
                s.SetMarker(this);
            SetOccupation(true);
            if (surface.CompareTag("Plate"))
            {
                PlateStock plateStock = GetComponentInParent<PlateStock>();
                plateStock.PlateList().Add(s);
                obj.transform.SetParent(transform.parent);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, .03f);     
    }

    public bool GetOccupation()
    {
        return isOccupied;
    }

    public void SetOccupation(bool _isOccupied)
    {
        isOccupied = _isOccupied;
    }
}
