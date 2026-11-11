//using UnityEngine;

//public class CombatManager : MonoBehaviour
//{
//    public enum States { Start, PlayerTurn, EnemyTurn, End, Won, Lost};

//    [SerializeField] int PlayerMaxHp;
//    [SerializeField] int EnemyMaxHp;
//    public States state;
//    public GameObject player;
//    public GameObject Enemy;



//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        state = States.Start;
//        //Combat Start // Setup
//    }

//    void PlayerTurn()
//    {
//        state = States.PlayerTurn;
//        //choose player action
//    }
//    void EnemyTurn()
//    {

//    }
//    void End()
//    {
//        if(state == States.Won)
//        {
//            //display Victory
//        }
//        else if(state == States.Lost)
//        {
//            //display loss
//        }
//    }
//}
