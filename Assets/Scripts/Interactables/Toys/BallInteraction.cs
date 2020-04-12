using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] NeedsType mNeedsType = NeedsType.BigBall;
    [SerializeField] HoldingObjectType mHoldingObjectType = HoldingObjectType.None;
    private bool isPickedUp = false;

    public InteractType GetInteractType()
    {
        return InteractType.Interact;
    }

    public Vector3 GetInteractionPoint()
    {
        return transform.position;
    }

    public void OnInteraction()
    {
        if (!isPickedUp) // so be picked up by the player
        {
            BeingPicked();
        }

        else // get dropped by the player
        {
            BeingDropped();
        }
    }

    public void BeingDropped()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        PlayerManager.instance.DropObject();
        isPickedUp = false;
        GetComponent<Rigidbody>().AddRelativeForce(new Vector3
            (Random.Range(10f, 20f), Random.Range(20f, 30f), Random.Range(10f, 20f))); // adds some random force for fun
    }

    public void BeingPicked()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        PlayerManager.instance.PickUpObject(this.gameObject);
        isPickedUp = true;
    }

    public GameObject GetInteractableGameObject()
    {
        return this.gameObject;
    }


    public NeedsType GetInteractableNeedsType()
    {
        return mNeedsType;
    }

    public HoldingObjectType GetHoldingObjectType()
    {
        return mHoldingObjectType;
    }
}
