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

    [SerializeField] public float jumpTimer;

    [SerializeField] public int jumpCount;
    [SerializeField] public float jumpCountMax;

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
        jump();
        movement();
    }
    void movement()
    {
        //get curr input and make sure that diagonal move is not accelerated
        Vector2 currInput = Vector2.ClampMagnitude(moveAct.ReadValue<Vector2>(), 1f);
        

        //vertical and horizontal
        Vector3 vert = transform.forward;
        Vector3 hori = transform.right;

        //temp without camera
        vert.y = 0;
        hori.y = 0;
        vert.Normalize();
        hori.Normalize();

        moveDir = (vert * currInput.y) + hori * currInput.x;


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


    //vertical jump and gravity
    void jump()
    {
        gravity = -Mathf.Abs(gravity);
        fallVelocity = -Mathf.Abs(fallVelocity);

        if (isGrounded)
        {
            jumpTimer = Time.time;
            jumpCount = 0;

            //keeps the player grounded while moveing even on slope's
            if (velocity.y <= 0f)
            {
                velocity.y = -1f;
            }
        }

        bool inCoyoteTime = Time.time - jumpTimer <= coyoteTime;
        if (!inCoyoteTime && jumpCount == 0)
        {
            jumpCount = 1;
        }

        if (jumpAct.WasPressedThisFrame() && jumpCount < jumpCountMax)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            jumpCount++;
            jumpTimer = 0f;


        }

        velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, fallVelocity);
    }

}
