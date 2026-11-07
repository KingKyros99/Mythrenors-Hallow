using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class EnemyNav : MonoBehaviour, Damage
{
    //variable's

    [SerializeField] int HP;

    [SerializeField] Renderer model;
    Color OrigColor;

    public GameObject player;
    public float Distance;
    private NavMeshAgent MeshAgent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get MeshAgent
        OrigColor = model.material.color;
        MeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Distance = Vector3.Distance(player.transform.position, this.transform.position);
        //if(Distance < 20)
        //{
        //    //switch to turn based combat scene

        //}
        //while player exist go to player
        if (player != null)
        {
            MeshAgent.SetDestination(player.transform.position);
        }
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            //destroy enemy
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DmgFlash());
        }
    }
    IEnumerator DmgFlash()
    {
        model.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        model.material.color = OrigColor;
    }
}
