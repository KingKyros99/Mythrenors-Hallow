using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{

    public Transform player;

    public MovementController playerMov;


    //camera offset and pos
    [SerializeField] Vector3 playerOffset;

    [SerializeField] float distance;

    [SerializeField] float shoulderDist;



    [SerializeField] float horizontalRot;

    [SerializeField] float verticalRot;
    


    [SerializeField] float camSensitivity;

    [SerializeField] float camRotMax;

    [SerializeField] float camRotMin;



    [SerializeField] float collisionZone;

    [SerializeField] float minDist;

    [SerializeField] float camSpeed;
   
    [SerializeField] float currShoulder;

    [SerializeField] float currDist;

    //Utility
    [SerializeField] private Camera cam;

    private InputAction lookAct;

    [SerializeField] LayerMask collisionLayer = 0;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        currDist = distance;
        currShoulder = Mathf.Abs(shoulderDist);

        lookAct = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");

    }

    private void OnEnable()
    {
        lookAct.Enable();
    }
    private void OnDisable()
    {
        lookAct.Disable();
    }



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        Vector3 playerRot = transform.eulerAngles;
        horizontalRot = playerRot.y;
        verticalRot = playerRot.x;


    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }


        if(Cursor.lockState == CursorLockMode.Locked)
        {
            
            Vector2 moveCam = lookAct.ReadValue<Vector2>() * camSensitivity;
            
            horizontalRot += moveCam.x;
            verticalRot -= moveCam.y;
            verticalRot = Mathf.Clamp(verticalRot, camRotMin, camRotMax);
        }



        player.rotation = Quaternion.Euler(0f, horizontalRot, 0f);
        
        
        Quaternion completeRot = Quaternion.Euler(verticalRot, horizontalRot, 0f);
        Quaternion horizontalRotation = Quaternion.Euler(0f, horizontalRot, 0f);

        bool backFace = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true; 

        Vector3 playerPivot = player.position + (horizontalRotation * playerOffset);

        float rightOffset = Mathf.Abs(shoulderDist);
        Vector3 finalPivot = playerPivot;
        if (rightOffset > 0.01f)
        {
            Vector3 shoulderDir = horizontalRotation * (Vector3.right * Mathf.Sign(shoulderDist));
            Vector3 origin = playerPivot - shoulderDir * 0.1f;
            if (Physics.SphereCast(origin, collisionZone, shoulderDir, out RaycastHit shoulderHit, rightOffset + 0.1f, collisionLayer, QueryTriggerInteraction.Ignore))
            {
                finalPivot = origin + shoulderDir * Mathf.Max(0f, shoulderHit.distance);
            }
            else
            {
                finalPivot = playerPivot + shoulderDir * rightOffset;
            }
        }
        

        Vector3 targetPos = finalPivot + (completeRot * Vector3.back * distance);
        Vector3 camDirection = completeRot * Vector3.back;

        //Colision
        float targetDistance = distance;

        //check in a spere to determine if the camera is already in or on wall
        if (Physics.SphereCast(finalPivot, collisionZone, camDirection, out RaycastHit hit, distance, collisionLayer, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(0f, hit.distance);
        }
        else if (Physics.Raycast(finalPivot, camDirection, out RaycastHit rayHit, distance + collisionZone, collisionLayer, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(0f, rayHit.distance - collisionZone);
        }
        Physics.queriesHitBackfaces = backFace;

        if (targetDistance < currDist)
        {
            currDist = targetDistance;
        }
        else
        {
            currDist = Mathf.MoveTowards(currDist, targetDistance, camSpeed * Time.deltaTime);

        }

        //final camPos and rotation
        Vector3 finalCamPos = finalPivot + camDirection * currDist;
        transform.position = finalCamPos;
        transform.rotation = completeRot;


        Debug.DrawLine(playerPivot, finalPivot, Color.green);
        Debug.DrawLine(finalPivot, transform.position, Color.red);

    }

}
