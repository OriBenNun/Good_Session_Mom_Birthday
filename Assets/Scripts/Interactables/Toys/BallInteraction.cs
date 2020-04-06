using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] NeedsType needsType = NeedsType.BigBall;
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
            GetComponent<Rigidbody>().isKinematic = true;
            PlayerManager.instance.PickUpObject(this.gameObject);
            isPickedUp = true;
        }

        else // get dropped by the player
        {
            GetComponent<Rigidbody>().isKinematic = false;
            PlayerManager.instance.DropObject();
            isPickedUp = false;
            GetComponent<Rigidbody>().AddRelativeForce(new Vector3
                (Random.Range(10f, 20f), Random.Range(20f, 30f), Random.Range(10f, 20f))); // adds some random force for fun
        }
    }

    public GameObject GetInteractableGameObject()
    {
        return this.gameObject;
    }


    public NeedsType GetInteractableNeedsType()
    {
        return needsType;
    }
}
