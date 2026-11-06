using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

public class BossDoor : MonoBehaviour
{
    //create a list of enemys
    //check enemy count 
    //open when enemy count = 0
    [SerializeField] private List<Enemy> minionCount = new List<Enemy>();
    [SerializeField] private Collider doorCollider;
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
        CheckObjective();
    }

    private void Update()
    {
        if (isDoorOpen)
        {
            return;
        }
        minionCount.RemoveAll(e => e == null);
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
        CheckObjective();
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
    }

}
