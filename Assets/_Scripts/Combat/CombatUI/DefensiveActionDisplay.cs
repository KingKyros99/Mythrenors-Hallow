using TMPro;
using UnityEngine;

// MYTHRENOR'S HALLOW - DEFENSIVE ACTION DISPLAY
//
// PURPOSE:
// Displays guarding and dodging states
// on the player's Combat Hotbar.
//
// NOTE:
// Display only. Does not activate defensive
// actions or modify stamina or combat behavior.

public class DefensiveActionDisplay : MonoBehaviour
{
    // Defensive Action Type
    public enum DefensiveActionType
    {
        Guard,
        Dodge
    }

    // Defensive References
    [SerializeField] private CoreFunctions coreFunctions;

    [SerializeField] private DefensiveActionType actionType;

    // UI References
    [SerializeField] private TMP_Text statusText;

    // Display Colors
    [SerializeField] private Color inactiveColor = Color.white;

    [SerializeField] private Color activeColor = Color.cyan;

    // Tracks the previous defensive state.
    private bool previousActive;
    private bool initialized;

    private void Awake()
    {
        // Find CoreFunctions if the display is
        // attached to the same character.
        if (coreFunctions == null)
        {
            coreFunctions = GetComponent<CoreFunctions>();
        }
    }

    private void OnEnable()
    {
        initialized = false;
    }

    // PURPOSE:
    // Updates the displayed defensive action
    // only when its state changes.
    private void Update()
    {
        if (coreFunctions == null || statusText == null)
        {
            return;
        }

        bool isActive = false;

        switch (actionType)
        {
            case DefensiveActionType.Guard:

                isActive = coreFunctions.IsBlocking;

                break;

            case DefensiveActionType.Dodge:

                isActive = coreFunctions.IsDodging;

                break;
        }

        if (initialized && isActive == previousActive)
        {
            return;
        }

        // Update the defensive status text.
        if (isActive)
        {
            statusText.text = actionType == DefensiveActionType.Guard ? "GUARDING" : "DODGING";

            statusText.color = activeColor;
        }

        else
        {
            statusText.text = "IDLE";

            statusText.color = inactiveColor;
        }

        previousActive = isActive;

        initialized = true;
    }
}
