using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DungeonMap : MonoBehaviour, IInteraction
{
    [Header("Map")]
    [SerializeField] private string interactText;
    [SerializeField] private Sprite Map;

    [SerializeField] private GameObject MapPanel;
    [SerializeField] private Image MapImage;

    private PlayerInteractions player;
    private bool isInspecting = false;
    private float mapOpenedTime;

    public string PromptInteraction(PlayerInteractions interaction)
    {
        return interactText;
    }
    public bool CanInteract(PlayerInteractions interaction)
    {
        return !isInspecting && !interaction.IsInteracting;
    }

    public void Interact(PlayerInteractions interaction)
    {
        Debug.Log("Interacted");
        player = interaction;
        player.IsInteracting = true;
        isInspecting = true;
        mapOpenedTime = Time.unscaledTime + 0.2f;

        if (MapImage != null)
        {
            MapImage.sprite = Map;
        }
        if (MapPanel != null)   
        {
            MapPanel.SetActive(true);
        }
    }

    public void CloseMap()
    {
        Debug.Log("ClosedMap");
        isInspecting = false;
        if (MapPanel != null)
        {
            MapPanel.SetActive(false);

            if (player != null)
            {
                player.IsInteracting = false;
                player = null;
            }


        }
    }

    
    void Start()
    {
        if (MapPanel != null)
        {
            MapPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInspecting || Time.unscaledTime - mapOpenedTime < 0.25f)
        {
            return;
        }
        

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseMap();
        }

       
    }
}
