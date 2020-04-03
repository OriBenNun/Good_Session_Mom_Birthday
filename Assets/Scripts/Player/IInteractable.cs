using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    Vector3 InteractionPoint(); // sets the position of the interaction
    InteractType GetInteractType(); // the player calls to know the type of interact
}

public enum InteractType
{
    Move,
    Interact
}
