using TMPro;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Find Interactions")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask interactionLayer;


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionPrompt;

    //inventory


    //active when interacting with objects like maps and bonfires
    public bool IsInteracting { get; set; } = false;
    private IInteraction currentInteractable;


    void Update()
    {
        if (IsInteracting == true)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
        //check if there is anything interactable in front of the player
        FindInteraction();
        //if find an interactable and press interact Interact
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    private void FindInteraction()
    {
        //create a raycast in front of the player
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactionLayer))
        {
            //check if raycast hit an interactable
            if (hit.collider.TryGetComponent(out IInteraction interaction) && interaction.CanInteract(this))
            {
                currentInteractable = interaction;
                interactionPrompt.text = $"[E] {interaction.PromptInteraction(this)}";
                interactionPrompt.gameObject.SetActive(true);
                return;
            }
        }
        //no interactions detected
        currentInteractable = null;
        interactionPrompt.gameObject.SetActive(false);
    }

}
