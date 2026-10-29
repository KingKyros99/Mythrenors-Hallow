using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
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



    [SerializeField] float colisionZone;

    [SerializeField] float minDist;

    [SerializeField] float camSpeed;    

    [SerializeField] float currDist;

    //Utility
    [SerializeField] Camera cam;

    InputAction lookAct;




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

        Quaternion rotation = Quaternion.Euler(verticalRot, horizontalRot, 0f);
        Vector3 playerPivot = player.position + playerOffset;

        Vector3 rightOffset = rotation * Vector3.right * shoulderDist;
        Vector3 finalPivot = playerPivot + rightOffset;

        Vector3 finalCamPos = finalPivot + (rotation * Vector3.back * currDist);

        //final camPos and rotation
        transform.position = finalCamPos;
        transform.rotation = rotation;
        player.rotation = Quaternion.Euler(0f, horizontalRot, 0f);
    }

}
