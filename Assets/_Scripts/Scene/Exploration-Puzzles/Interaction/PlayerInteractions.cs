using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Find Interactions")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask interactionLayer;


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionPrompt;



    //inventory
    [SerializeField] private List<string> allKeys = new List<string>();
    public List<string> Keys => allKeys;


    //Input
    [SerializeField] private InputAction input = new InputAction("Interaction", InputActionType.Button, "<Keyboard>/e");

    //active when interacting with objects like maps and bonfires
    public bool IsInteracting { get; set; } = false;
    private IInteraction currentInteractable;


    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }


    void Update()
    {
        if (IsInteracting == true)
        {
            interactionPrompt.gameObject.SetActive(false);
            return;
        }
        //check if there is anything interactable in front of the player
        FindInteraction();
        //if find an interactable and press interact Interact
        if (currentInteractable != null && input.WasPressedThisFrame())
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
