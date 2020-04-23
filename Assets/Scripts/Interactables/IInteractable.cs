using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnInteraction(); // the individual method of interaction

    IEnumerator OnFulfilledNeedBehaviour(NeedsAISystem client); // the object behaviour when a client fulfilled a need with that object

    Vector3 GetInteractionPoint(); // sets the position of the interaction
    InteractType GetInteractType(); // the player calls to know the type of interact

    GameObject GetInteractableGameObject(); // the player calls to get the gameobject of the interactable

    NeedsType GetInteractableNeedsType(); // what object is that need by Need Type to compare with client needs

    HoldingObjectType GetHoldingObjectType();

    void FadeObject(bool shouldFade, float speed = 1); // used to fade out and in objects, using the DissolveMaterialCreatorController

    bool GetIsCurrentlyInteractable(); // for the player to know if a object is currently used by a client;
}

public enum InteractType
{
    Move,
    Interact
}
