using UnityEngine;

public interface IInteractable
{
    bool CanInteract(GameObject interactor); // Checks if the object can be interacted with at the moment

    void Interact(GameObject interactor); // Perform the interaction. 

}
