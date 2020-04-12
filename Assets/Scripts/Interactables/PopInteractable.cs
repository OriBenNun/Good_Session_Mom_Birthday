using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopInteractable : MonoBehaviour , IInteractable
{
    private Transform clientTransform;

    private void Start()
    {
        clientTransform = GetComponentInParent<NeedsAISystem>().transform;
    }

    public void OnInteraction()
    {
        // TODO add help label with the object name / zoon in?
        clientTransform.GetComponent<NeedsAISystem>().OnInteraction(); // calls on the Client, like the player pressed him
    }

    public Vector3 GetInteractionPoint()
    {
        return clientTransform.GetComponent<NeedsAISystem>().GetInteractionPoint();
    }

    public InteractType GetInteractType()
    {
        return InteractType.Interact;
    }

    public GameObject GetInteractableGameObject()
    {
        return this.gameObject;
    }

    public NeedsType GetInteractableNeedsType()
    {
        return clientTransform.GetComponent<NeedsAISystem>().GetCurrentNeed().GetNeedsType();
    }

    public HoldingObjectType GetHoldingObjectType()
    {
        throw new System.NotImplementedException();
    }
}
