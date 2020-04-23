using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour, IInteractable
{
    public InteractType GetInteractType()
    {
        return InteractType.Move;
    }

    public Vector3 GetInteractionPoint()
    {
        return transform.position;
    }

    public void OnInteraction()
    {
        return;
    }

    public GameObject GetInteractableGameObject()
    {
        return this.gameObject;
    }

    public NeedsType GetInteractableNeedsType()
    {
        return NeedsType.None;
    }

    public HoldingObjectType GetHoldingObjectType()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator OnFulfilledNeedBehaviour(NeedsAISystem client)
    {
        throw new System.NotImplementedException();
    }

    public void FadeObject(bool shouldFade, float speed = 1)
    {
        throw new System.NotImplementedException();
    }

    public bool GetIsCurrentlyInteractable()
    {
        throw new System.NotImplementedException();
    }
}
