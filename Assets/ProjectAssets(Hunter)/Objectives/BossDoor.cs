using UnityEngine;
using System.Collections.Generic;
using System.Text;
using TMPro;
public class BossDoor : MonoBehaviour
{
    //create a list of enemys
    //check enemy count 
    //open when enemy count = 0
    [SerializeField] private List<Enemy> minionCount = new List<Enemy>();
    [SerializeField] private Collider doorCollider;
    [SerializeField] private TMP_Text objectiveBoard;
    private bool isDoorOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Enemy e in minionCount)
        {
            if (e != null)
            {
                e.bossDoor = this;
            }
        }
        ObjectiveBoard();
        CheckObjective();
    }

    private void Update()
    {
        if (isDoorOpen)
        {
            return;
        }
        minionCount.RemoveAll(e => e == null);
        ObjectiveBoard();
        CheckObjective();
    }

    public void addEnemy(Enemy enemy)
    {
        minionCount.Add(enemy);
        enemy.bossDoor = this;
    }
    public void RemoveEnemy(Enemy enemy)
    {
        minionCount.RemoveAll(e => e == null || e == enemy);
        ObjectiveBoard();
        CheckObjective();
    }





    void ObjectiveBoard()
    {
        if (objectiveBoard == null)
        {
            return;
        }
        string text = $"Enemies Remaining: {minionCount.Count}";
        objectiveBoard.text = text;
    }


    void CheckObjective()
    {
        if (minionCount.Count == 0 && !isDoorOpen) 
        {
            DoorOpen();
        }
        else
        {
            return;
        }
    }
  
    
    void DoorOpen()
    {
        isDoorOpen = true;
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        if (objectiveBoard != null)
        {
            objectiveBoard.text = "Gate was Opened";
        }

    }

}

