using UnityEngine;
using UnityEditor.Rendering;
public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController Controller;
    
    [Range(1,100)][SerializeField] int Health;
    [Range(1,20)][SerializeField] int speed;
    [Range(1,200)][SerializeField] int jumpSpeed;
    [Range(1, 30)][SerializeField] int Gravity;
    int OrigHP;
    int doubleJump;

    [Range(1,100)][SerializeField] int AtkDmg;
    [Range(1,100)][SerializeField] int range;
    [Range(0.1f, 15)][SerializeField] float AtkRate;
    float attackTimmer;


    //vectors for moving the player and jumping
    Vector3 Direction;
    Vector3 PlayerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OrigHP = Health;
    }

    // Update is called once per frame
    void Update()
    {
        movement();
    }

    void movement()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * range, Color.red);
        attackTimmer += Time.deltaTime;


        //if on ground set to 0
        if (Controller.isGrounded)
        {
            doubleJump = 0;
            PlayerVel.y = 0;
        }
        //toggles which direction the player should move
        Direction = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        Controller.Move(Direction.normalized * speed * Time.deltaTime);

        //call jump and change palyerVel by greavity;
        jump();
        Controller.Move(PlayerVel * Time.deltaTime);
        PlayerVel.y -= Gravity + Time.deltaTime;

        if(Input.GetButtonDown("Fire1") && attackTimmer > AtkRate)
        {
            Attack();
        }
    }
    void jump()
    {
        //while jump count < 2 Jump and increse jump count
        if (Input.GetButtonDown("Jump") && doubleJump < 2)
        {
            doubleJump++;
            PlayerVel.y = jumpSpeed;
        }
    }
    void Attack()
    {
        attackTimmer = 0;
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range))
        {
            Debug.Log(hit.collider.name);
            Damage Dmg = hit.collider.GetComponent<Damage>();
            if (Dmg != null)
            {
                Dmg.takeDamage(AtkDmg);
            }
        }
    }
}
