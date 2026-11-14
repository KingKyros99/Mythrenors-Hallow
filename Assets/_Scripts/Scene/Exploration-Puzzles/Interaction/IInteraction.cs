using UnityEngine;

public interface IInteraction 
{
    //Displays the Interaction propmt on screen
    string PromptInteraction(PlayerInteractions interaction);
    //Checks if an object can be interacted with
    bool CanInteract(PlayerInteractions interaction);
    //Where the actual interaction happens
    void Interact(PlayerInteractions interaction);
}
