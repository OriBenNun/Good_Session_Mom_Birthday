using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopInteractable : MonoBehaviour , IInteractable
{
    private NeedsAISystem m_Client;

    private void Start()
    {
        m_Client = GetComponentInParent<NeedsAISystem>();
    }

    public void OnInteraction()
    {
        // TODO add help label with the object name / zoon in?
        m_Client.OnInteraction(); // calls on the Client, like the player pressed him
    }

    public Vector3 GetInteractionPoint()
    {
        return m_Client.GetInteractionPoint();
    }

    public InteractType GetInteractType()
    {
        return m_Client.GetInteractType();
    }

    public GameObject GetInteractableGameObject()
    {
        return m_Client.GetInteractableGameObject();
    }

    public NeedsType GetInteractableNeedsType()
    {
        return m_Client.GetCurrentNeed().GetNeedsType();
    }

    public HoldingObjectType GetHoldingObjectType()
    {
        throw new System.NotImplementedException();
    }

    public void FadeObject(bool toggle, float speed = 1)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator OnFulfilledNeedBehaviour(NeedsAISystem client)
    {
        throw new System.NotImplementedException();
    }

    public bool GetIsCurrentlyInteractable()
    {
        return m_Client.GetIsCurrentlyInteractable();
    }
}
