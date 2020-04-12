using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnInteraction(); // the individual method of interaction

    Vector3 GetInteractionPoint(); // sets the position of the interaction
    InteractType GetInteractType(); // the player calls to know the type of interact

    GameObject GetInteractableGameObject(); // the player calls to get the gameobject of the interactable

    NeedsType GetInteractableNeedsType(); // what object is that need by Need Type to compare with client needs

    HoldingObjectType GetHoldingObjectType();
}

public enum InteractType
{
    Move,
    Interact
}
