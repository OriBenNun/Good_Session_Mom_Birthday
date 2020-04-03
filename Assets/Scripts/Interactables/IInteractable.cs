using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnInteraction(); // the special method of interaction

    Vector3 GetInteractionPoint(); // sets the position of the interaction
    InteractType GetInteractType(); // the player calls to know the type of interact


    GameObject GetInteractableGameObject(); // the player calls to get the gameobject of the interactable
}

public enum InteractType
{
    Move,
    Interact
}
