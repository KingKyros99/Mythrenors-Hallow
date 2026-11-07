using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public CharacterController controller;
    private CharacterBase character;

    // PURPOSE:
    // Provides dodge displacement without handling
    // stamina, invulnerability or combat damage.
    private DodgeMovement dodgeMovement;

    //Movement/Sprint Var.
    [Header("sprint")]

    [SerializeField] public float baseSpeed;

    [SerializeField] public float sprintSpeed;

    [SerializeField] public float moveResistance;

    [SerializeField] private float sprintStaminaCost = 2f;

    [SerializeField] private float minStamina;

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

    [Header("AI Movement")]
    [SerializeField] private bool aiControlled = false;

    private Vector3 aiMoveDirection;

    //Bools

    //Check if the character is sprinting
    public bool isSprinting { get; private set; }

    //check if the player controller is grounded
    public bool isGrounded => controller.isGrounded;

    // PURPOSE:
    // Gets the current movement direction so
    // CombatInputController can begin a directional dodge.
    public Vector3 CurrentMoveDirection
    {
        get
        {
            if (aiControlled)
            {
                Vector3 aiDirection = aiMoveDirection;

                aiDirection.y = 0f;

                return Vector3.ClampMagnitude(aiDirection, 1f);
            }

            if (moveAct == null)
            {
                return Vector3.zero;
            }

            Vector2 input = Vector2.ClampMagnitude(moveAct.ReadValue<Vector2>(), 1f);

            Vector3 direction = transform.forward * input.y + transform.right * input.x;

            direction.y = 0f;

            return Vector3.ClampMagnitude(direction, 1f);
        }
    }

    private void Awake()
    {
        character = GetComponent<CharacterBase>();

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
        jumpAct.AddBinding("<Keyboard>/space");

        dodgeMovement = GetComponent<DodgeMovement>();
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

    // Made updates to it to allow dodging into a directional way.
    void movement()
    {
        // Read movement input for the player.
        Vector2 currInput = Vector2.zero;

        if (!aiControlled && moveAct != null)
        {
            currInput = Vector2.ClampMagnitude(moveAct.ReadValue<Vector2>(), 1f);
        }

        // Use the existing movement direction.
        moveDir = CurrentMoveDirection;

        // Check whether dodge displacement is active.
        bool dodgeActive = dodgeMovement != null && dodgeMovement.IsMoving;

        // Sprinting is disabled during dodging.
        if (aiControlled || dodgeActive)
        {
            isSprinting = false;
        }
        else
        {
            SprintStamina(currInput);
        }

        // PURPOSE:
        // Dodge movement replaces normal horizontal
        // movement but preserves vertical gravity.
        if (dodgeActive)
        {
            Vector3 dodgeStep = dodgeMovement.ConsumeFrameMovement(Time.deltaTime);

            // Remove old walking or sprinting momentum.
            velocity.x = 0f;

            velocity.z = 0f;

            // Keep gravity active during the dodge.
            dodgeStep.y = velocity.y * Time.deltaTime;

            controller.Move(dodgeStep);

            return;
        }

        // Normal walking or sprinting.
        float currentSpeed = isSprinting ? sprintSpeed : baseSpeed;

        Vector3 horizontalDir = moveDir * currentSpeed;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        if (moveResistance <= 0f)
        {
            horizontal = horizontalDir;
        }

        else
        {
            horizontal = Vector3.MoveTowards(horizontal, horizontalDir, moveResistance * Time.deltaTime);
        }

        velocity.x = horizontal.x;

        velocity.z = horizontal.z;

        // Apply movement once per frame.
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

        // PURPOSE:
        // Prevent jumping while dodge movement is active.
        bool dodgeActive = dodgeMovement != null && dodgeMovement.IsMoving;

        if (!dodgeActive && jumpAct.WasPressedThisFrame() && jumpCount < jumpCountMax)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpCount++;

            jumpTimer = 0f;
        }

        velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, fallVelocity);
    }


    private void SprintStamina(Vector2 currInput)
    {
        if (character == null)
        {
            isSprinting = false;
            return;
        }
        bool isMoving = currInput.sqrMagnitude > 0.1f;

        bool isSprintDown = sprintAct.IsPressed() && isMoving;

        float sprintTime = sprintStaminaCost * Time.deltaTime;

        float neededStamina = isSprinting ? sprintTime : minStamina;

        isSprinting = isSprintDown && character.CurrentStamina >= neededStamina;

        if (isSprinting)
        {


            // Stop sprinting when stamina runs out.
            if (!character.SpendStamina(sprintTime))
            {
                isSprinting = false;
            }
        }
    }

    public void SetAIMovement(Vector3 direction)
    {
        aiControlled = true;
        aiMoveDirection = Vector3.ClampMagnitude(direction, 1f);
    }
}
