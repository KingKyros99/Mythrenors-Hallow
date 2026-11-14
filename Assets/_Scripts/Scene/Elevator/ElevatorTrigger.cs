using UnityEngine;



//How the player/entity triggers elevator movement
public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] ElevatorMovement elevator;
    private void Start()
    {
        if (elevator != null)
        {
            return;
        }
        elevator = elevator.GetComponent<ElevatorMovement>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && elevator != null)
        {
            other.transform.SetParent(transform);
            if (elevator.canMove == false)
            {
                elevator.canMove = true;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }

}
