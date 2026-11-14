using UnityEngine;
using System.Collections.Generic;
using System.Text;
using TMPro;
public class Door : MonoBehaviour, IInteraction
{
    [Header("Locked")]
    [SerializeField] private bool isLocked;
    [SerializeField] private string keyNeeded;
    [SerializeField] private bool keepKey = false;

    [Header("Dungeon Door Open")]
    [SerializeField] private Transform hinge;
    [SerializeField] private float angle;
    [SerializeField] private float speed;

    [SerializeField] private Collider doorCollider;
    [SerializeField] private TMP_Text objectiveBoard;

    private bool isDoorOpen;

    private Quaternion closedRot;
    private Quaternion openRot;




    private void Awake()
    {
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider>();

        }
    }


    void Start()
    {
        if (hinge == null)
        {
            hinge = transform;
        }

        closedRot = hinge.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, 0f, angle);


        ObjectiveBoard();
        
    }

    private void Update()
    {
        if (hinge != null)
        {
            Quaternion targetRot = isDoorOpen ? openRot : closedRot;
            hinge.localRotation = Quaternion.Slerp(hinge.localRotation, targetRot, Time.deltaTime * speed);
        }

        if (isDoorOpen)
        {
            return;
        }
        //checks the boss doors open condion and displays it on the objective obj
        ObjectiveBoard();
        
    }
    public string PromptInteraction(PlayerInteractions interaction)
    {
        return interaction.Keys.Contains(keyNeeded) ? "Unlock Boss Gate" : $"Locked ({keyNeeded} Required)";
    }
    public bool CanInteract(PlayerInteractions interaction)
    {
        return isLocked && !interaction.IsInteracting;
    }
    public void Interact(PlayerInteractions interaction)
    {
        if (!isLocked)
        {
            return;
        }
        if (interaction.Keys.Contains(keyNeeded))
        {
            isLocked = false;
            if (keepKey == true)
            {
                interaction.Keys.Remove(keyNeeded);
            }
            DoorOpen();
        }
    }


    void ObjectiveBoard()
    {
        if (objectiveBoard == null)
        {
            return;
        }
        string text = isDoorOpen ? "Gate is Open" : $"Find {keyNeeded}" ;
        objectiveBoard.text = text;
    }

    
    void DoorOpen()
    {
        isDoorOpen = true;
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        ObjectiveBoard();

    }

}

