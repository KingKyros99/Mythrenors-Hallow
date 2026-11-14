using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorMovement : MonoBehaviour
{
    //Elevator Fields
    [SerializeField] public bool canMove;
    [SerializeField] public float speed;
    [SerializeField] int startPoint;
    [SerializeField] Transform[] points;

    [SerializeField] private string elevatorId;
    [Serializable] private class ElevatorSaveDate
    {
        public Vector3 position;
        public int index;
        public bool moveBack;
        public bool canMove;
    }
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, $"elevator_{SceneManager.GetActiveScene().name}_{elevatorId}.json");

    private int index;
    private bool moveBack;


    private void OnEnable()
    {
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved += SaveElevator;
        }
    }
    
    private void OnDisable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved -= SaveElevator;
        }   
    }

    void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved -= SaveElevator;
            SaveManager.Instance.Saved += SaveElevator;
            
        }
        bool hasSave = PlayerPrefs.GetInt("mh.save.hasGame", 0) == 1;
        if (hasSave && File.Exists(SaveFilePath))
        {
            LoadElevator();
        }
        else
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
            //set elevator to its start position
            transform.position = points[startPoint].position;
            index = startPoint;
        }
            
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


    private void SaveElevator()
    {
        ElevatorSaveDate save = new ElevatorSaveDate
        {
            position = transform.position,
            index = index,
            moveBack = moveBack,
            canMove = canMove
        };

        File.WriteAllText(SaveFilePath, JsonUtility.ToJson(save, true));
    }
    private void LoadElevator()
    {
        string file = File.ReadAllText(SaveFilePath);
        ElevatorSaveDate data = JsonUtility.FromJson<ElevatorSaveDate>(file);
        if (data != null)
        {
            transform.position = data.position;
            index = data.index;
            moveBack = data.moveBack;
            canMove = data.canMove;
        }

    }

}
