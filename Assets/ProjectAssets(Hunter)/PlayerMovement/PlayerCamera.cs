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

    [SerializeField] float currDist;

    //Utility
    [SerializeField] Camera cam;

    InputAction lookAct;

    [SerializeField] LayerMask collisionLayer = -0;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        currDist = distance;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        Vector3 playerPos = player.position;
        Vector3 playerPivot = playerPos + horizontalRotation * playerOffset;

        Vector3 rightOffset = horizontalRotation * Vector3.right * shoulderDist;
        Vector3 finalPivot = playerPivot + rightOffset;

        Vector3 camDirection = (completeRot * Vector3.back).normalized;

        //Colision
        float targetDistance = distance;
        if (Physics.SphereCast(finalPivot, collisionZone, camDirection, out RaycastHit hit, distance, collisionLayer))
        {
            targetDistance = Mathf.Clamp(hit.distance - 0.1f, minDist, distance);
        }
        currDist = Mathf.MoveTowards(currDist, targetDistance, camSpeed * Time.deltaTime);

        //final camPos and rotation
        Vector3 finalCamPos = finalPivot + (camDirection * currDist);
        transform.position = finalCamPos;
        transform.rotation = completeRot;
        
    }

}
