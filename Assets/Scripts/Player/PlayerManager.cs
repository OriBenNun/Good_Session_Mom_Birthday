using System;
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance; // Singelton


    [Header("Picking Up Settings")]
    [SerializeField] GrabbingPoint grabbingPoint = null;
    [SerializeField] Transform interactablesParent = null;

    [Header("Player's Settings")]
    [SerializeField] float doublebackExitTimer = 1f;

    public event Action OnPlayerFinishedGiveAnimation;

    private bool isHoldingSomething = false;
    private IInteractable currentInteractableHold = null;

    private float exitTimer = Mathf.Infinity;

    private PlayerAnimatorController mAnimatorController;

    private float pickupTimeInAnimation = 0.5f;

    private void Awake()
    {
        if (PlayerManager.instance == null)
        {
            instance = this;
        }
        mAnimatorController = GetComponent<PlayerAnimatorController>();
    }

    private void Update()
    {
        DoubleBackExitApp();
    }

    private void DoubleBackExitApp()
    {
        if (exitTimer < doublebackExitTimer)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitTimer = 0;
        }
        exitTimer += Time.deltaTime;
    }

    public GameObject GetPlayerGameObject()
    {
        return this.gameObject;
    }

    public void PickUpObject(GameObject interactable)
    {
        if (isHoldingSomething)
        {
            // tells the current held object to get dropped.
            //the function in the interactable is calling this script's Drop function
            currentInteractableHold.OnInteraction();
        }

        if (!isHoldingSomething)
        {
            StartCoroutine("PickingUpSequence", interactable);
        }
    }

    private IEnumerator PickingUpSequence(GameObject interactable)
    {
        var objectHoldingType = interactable.GetComponent<IInteractable>().GetHoldingObjectType();
        mAnimatorController.PickupAnimation(objectHoldingType);
        yield return new WaitForSeconds(pickupTimeInAnimation);

        isHoldingSomething = true;
        interactable.transform.position = grabbingPoint.transform.position;
        interactable.transform.parent = grabbingPoint.transform;
        currentInteractableHold = interactable.GetComponent<IInteractable>();
    }

    public void DropObject()
    {
        if (currentInteractableHold == null) { return; }
        isHoldingSomething = false;
        currentInteractableHold.GetInteractableGameObject().transform.parent = interactablesParent;
        currentInteractableHold = null;
        mAnimatorController.ResetAnimToLoco(); // resets the holding animation
    }

    public void GiveObject()
    {
        if (currentInteractableHold == null) { return; }
        mAnimatorController.GiveItemTrigger();
    }

    public void SuccesfulClientNeedFulfilled()
    {
        // TODO UI and VFX
    }

    public IInteractable GetIInteractableHeld()
    {
        return currentInteractableHold;
    }

    public void FinishedGiveAnimation()
    {
        OnPlayerFinishedGiveAnimation?.Invoke();

        isHoldingSomething = false;
        currentInteractableHold.GetInteractableGameObject().transform.parent = interactablesParent;
        currentInteractableHold = null;
        mAnimatorController.ResetAnimToLoco(); // resets the holding animation
    }
}
