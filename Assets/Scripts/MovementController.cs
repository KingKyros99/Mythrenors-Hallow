using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public CharacterController controller;
    
    //Movement/Sprint Var.
    [SerializeField] public float baseSpeed;

    [SerializeField] public float sprintSpeed;

    [SerializeField] public float moveResistance;

    Vector3 moveDir;
    Vector3 velocity;

    //Jump Var.
    [SerializeField] public float jumpHeight;

    [SerializeField] public float gravity;
    
    [SerializeField] public float fallVelocity;
    
    [SerializeField] public float coyoteTime;

    int jumpCount;

    InputAction moveAct, sprintAct, jumpAct;


    //Bools

    //Check if the character is sprinting
    public bool isSprinting { get; private set; }

    //check if the player controller is grounded
    public bool isGrounded => controller.isGrounded;



    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        //Move Action for new input system
        moveAct = new InputAction("Move", InputActionType.Value);
        moveAct.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");

        //Sprint Action for new input system
        sprintAct = new InputAction("Sprint", InputActionType.Button);
        sprintAct.AddBinding("<Keyboard>/leftShift");


        jumpAct = new InputAction("Jump", InputActionType.Button);
        jumpAct.AddBinding( "<Keyboard>/space");
    }

    private void OnEnable()
    {
        moveAct.Enable();
        jumpAct.Enable();
        sprintAct.Enable();
    }

    private void OnDisable()
    {
        moveAct.Disable();
        sprintAct.Disable();
        jumpAct.Disable();
    }

    void Update()
    {
        movement();
    }
    void movement()
    {
        //get curr input and make sure that diagonal move is not accelerated
        Vector2 currInput = Vector2.ClampMagnitude(moveAct.ReadValue<Vector2>(), 1f);
        

        //vertical and horizontal
        Vector3 vert;
        Vector3 hori;

        //temp without camera
        vert = Vector3.forward;
        hori = Vector3.right;
        moveDir = vert * currInput.y + hori * currInput.x;


        Vector3 horizontalDir = moveDir * baseSpeed;
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        
        if (moveResistance <= 0)
        {
            horizontal = horizontalDir;
        }
        else
        {
            horizontal = Vector3.MoveTowards(horizontal, horizontalDir, moveResistance * Time.deltaTime);
        }
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        controller.Move(velocity * Time.deltaTime);
       

    }
}
