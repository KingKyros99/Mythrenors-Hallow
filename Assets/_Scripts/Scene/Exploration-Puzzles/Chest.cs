using UnityEngine;

public class Chest : MonoBehaviour, IInteraction
{

    [Header("Hinge")]
    [SerializeField] private Transform chestLid;
    [SerializeField] private float angle;
    [SerializeField] private float speed;

    [SerializeField] private string interactionPrompt;

    private Collider contentColider;
    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (chestLid != null)
        {
            closedRot = chestLid.localRotation;
            openRot = closedRot * Quaternion.Euler(angle, 0f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (chestLid == null)
        {
            return;
        }
        Quaternion targetRot = isOpen ? openRot : closedRot;
        chestLid.localRotation = Quaternion.Slerp(chestLid.localRotation, targetRot, Time.deltaTime * speed);
    }


    public bool CanInteract(PlayerInteractions interaction)
    {
        if (isOpen)
        {
            return false;
        }
        return true;
    }

    public void Interact(PlayerInteractions interaction)
    {
        if (!isOpen)
        {
            isOpen = true;
        }
        if (TryGetComponent(out Collider chest))
        {
            chest.enabled = false;

        }
        if (contentColider != null)
        {
            contentColider.enabled = true;
        }
    }

    public string PromptInteraction(PlayerInteractions interaction)
    {
        return interactionPrompt;
    }

    
}
