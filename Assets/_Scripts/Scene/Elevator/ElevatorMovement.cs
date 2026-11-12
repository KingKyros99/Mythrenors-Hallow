using UnityEngine;

public class ElevatorMovement : MonoBehaviour
{
    //Elevator Fields
    [SerializeField] public bool canMove;
    [SerializeField] public float speed;
    [SerializeField] int startPoint;
    [SerializeField] Transform[] points;

    private int index;
    private bool moveBack;

    void Start()
    {
        //set elevator to its start position
        transform.position = points[startPoint].position;
        index = startPoint;
    }

    void Update()
    {
        //check if elevator reaches its destination
        if (Vector3.Distance(transform.position, points[index].position) == 0f)
        {
            //stop the elevator and change index
            canMove = false;
            if (index == points.Length - 1)
            {
                moveBack = true;
                index--;
                return;
            }
            else if (index == 0)
            {
                moveBack = false;
                index++;
                return;
            }

            if (moveBack == true)
            {
                index--;
            }
            else
            {
                index++;
            }

        }


        if (canMove == true)
        {
            //move elevator the destination point
            transform.position = Vector3.MoveTowards(transform.position, points[index].position, speed * Time.deltaTime);
        }
    }
}
