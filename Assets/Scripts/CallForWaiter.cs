using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CallForWaiter : MonoBehaviour
{
    public GameObject waiter;
    private NavMeshAgent agent;
    public Transform[] doors;
    private Transform[] hinge = new Transform[2];
    public Transform start,end;
    private bool inProgress = false;
    private Transform plateSlot;
    private Transform shoulder;

    void Start()
    {
        for(int i = 0; i < doors.Length; i++)
            hinge[i] = doors[i].transform.Find("Hinge");
    }

    IEnumerator DoorMovement(float angle) //coroutine for the doors to open
    {
        for (int i = 0; i < doors.Length; i++)
        {
            yield return StartCoroutine(RotateAnObject(Vector3.up, doors[i], hinge[i], angle, .5f));
            angle = -angle;
        }
        yield return null;
    }

    public IEnumerator GetWaiter(Transform toPos, Move_S plate) //routine to dictate the path and movement of the waiter
    {
        while (inProgress)
            yield return null;
        inProgress = true;

        GameObject instance =  Instantiate(waiter, start.position, new Quaternion(0, 180, 0, 0)); //new gameobject each time, quick way to revert to deafault transforms
        agent = instance.GetComponent<NavMeshAgent>(); //using navmesh agent to dictate the movemnt of the waiter 
        plateSlot = instance.transform.Find("PlateSlot");
        shoulder = instance.transform.Find("ArmNShoulder");

        StartCoroutine(DoorMovement(Vector3.up, 120, 1));
        yield return StartCoroutine(MoveWaiter(toPos, plate, instance));
        inProgress = false;
    }

    IEnumerator MoveWaiter(Transform areaPos, Move_S plate, GameObject waiter)
    {
        yield return agent.SetDestination(areaPos.position + Vector3.forward); //moving waiter to table

        while (agent.remainingDistance > agent.stoppingDistance) //making sure its close enough
            yield return null;
        
        yield return StartCoroutine(RotateAnObject(agent.transform.right, shoulder, shoulder, -60, .2f));
        plate.CheckForFood();
        plate.DisablePhysics(plateSlot);
        plate.GetMarker().SetOccupation(false);//getting access to objects physics
        yield return StartCoroutine(Move(plate.transform, .3f));
        yield return agent.SetDestination(end.position);
        yield return new WaitForSeconds(1);
        StartCoroutine(DoorMovement(Vector3.up, -120, 1));
        while (agent.remainingDistance > .1f)
            yield return null;
        Destroy(waiter);
    }

    public IEnumerator Move(Transform fromPos, float dur)
    {
        float i = 0;
        Vector3 startPos = fromPos.position;
        while (i < dur)
        {
            i += Time.deltaTime;
            if (fromPos != null)
            {
                fromPos.position = Vector3.Lerp(startPos, plateSlot.position, i / dur);
                yield return null;
            }
        }
    }

    IEnumerator DoorMovement(Vector3 axis, float angle, float dur)
    {
        float elapsed = 0;
        float rotated = 0;
        while (elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            doors[0].transform.RotateAround(hinge[0].position, axis, step);
            doors[1].transform.RotateAround(hinge[1].position, axis, -step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        for(int i = 0; i < doors.Length; i++)
        {
            doors[i].transform.RotateAround(hinge[i].position, axis, angle - rotated);
        }        
    }

    IEnumerator RotateAnObject(Vector3 axis, Transform obj,Transform centre, float angle, float dur)
    {
        float elapsed = 0;
        float rotated = 0;
        while (elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            obj.transform.RotateAround(centre.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        obj.transform.RotateAround(centre.position, axis, angle - rotated);        
    }
}
