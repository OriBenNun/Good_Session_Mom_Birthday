using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour, IInteractable
{
    public InteractType GetInteractType()
    {
        return InteractType.Move;
    }

    public Vector3 InteractionPoint()
    {
        return transform.position;
    }
}
