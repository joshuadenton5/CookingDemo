using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    private Vector2 mouseLook;
    private Vector2 smoothV;
    public float sensitivity = 5;
    public float smoothing = 2;
    private GameObject player;
    private bool move = true;

    void Start()
    {
        player = transform.parent.gameObject;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool Move()
    {
        return move;
    }
    public void AllowMove(bool m)
    {
        move = m;
    }

    void LateUpdate()
    {
        if (move)
            MouseControl();
    }

    void MouseControl() //allows the player to look around with the mouse 
    {
        Vector2 md = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
        mouseLook += smoothV;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -70f, 90f); //locking how far the player can look up/down

        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
    }
}
