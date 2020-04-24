using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineInteractable : MonoBehaviour , IInteractable
{
    [SerializeField] NeedsType mNeedsType = NeedsType.Trampoline;
    [SerializeField] HoldingObjectType mHoldingObjectType = HoldingObjectType.Client;
    [SerializeField] Transform interactPoint = null;

    private bool isPickedUp = false;

    private bool isInClientUse = false;
    private bool isInBetweenStates = false; // another bool to squish some bugs when player is spam clicking the ball at the end of animation
    private bool ableToReposition = false;

    DissolveMaterialCreatorController dissolver;
    AnimatorManager mAnimator;


    private const string startTriggerString = "start_ChildJumping";

    private const string finishTriggerString = "finish_ChildJumping";

    private void Awake()
    {
        dissolver = GetComponentInChildren<DissolveMaterialCreatorController>();
        mAnimator = GetComponent<AnimatorManager>();
    }

    public InteractType GetInteractType()
    {
        return InteractType.StaticToy;
    }

    public Vector3 GetInteractionPoint()
    {
        return interactPoint.position;
    }

    public void OnInteraction()
    {
        if (isInClientUse) { return; }

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
        mAnimator.ToggleNavAndKinematic(false);

        if (PlayerManager.instance.GetIInteractableHeld() == this)
        {
            PlayerManager.instance.DropObject();
        }

        isPickedUp = false;

        isInBetweenStates = false;
    }

    public void BeingPicked()
    {
        mAnimator.ToggleNavAndKinematic(true);
        PlayerManager.instance.PickUpObject(this.gameObject);
        isPickedUp = true;

        isInBetweenStates = false;
    }

    public void BeingGived()
    {
        PlayerManager.instance.GiveObject();
        isInClientUse = true;
        //isPickedUp = false;
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

    public IEnumerator OnFulfilledNeedBehaviour(NeedsAISystem client)
    {
        isInBetweenStates = true;


        BeingGived();

        var position = client.GetStartAnimationPosition(this.mNeedsType);

        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // reset the bool

        mAnimator.ToggleKinematicAndMoveToPosition(position, true);

        mAnimator.PlayTriggerAnimationSync(startTriggerString);

        yield return new WaitUntil(() => ableToReposition); // kinda like random wait. will be reset at the end of loops fade out, so we could set the finish trigger, which will be actually triggered by the client when all the loops are finished

        ableToReposition = false; // reset the bool

        mAnimator.TriggerAnimationNoSync(finishTriggerString);

        isInClientUse = false;
    }

    public void FadeObject(bool shouldFade, float speed = 1)
    {
        if (dissolver == null) { Debug.LogError("Hey! missing a dissolver on " + name); return; }

        if (shouldFade && dissolver.GetIsVisible())
        {
            isInBetweenStates = true;

            dissolver.StartDissolve();
            dissolver.OnFinishedDissolve += OnFinishedDissolveEvent;
        }

        else if (!shouldFade && !dissolver.GetIsVisible())
        {
            dissolver.StartReverseDissolve();
        }
    }

    private void OnFinishedDissolveEvent()
    {
        ableToReposition = true;

        dissolver.OnFinishedDissolve -= OnFinishedDissolveEvent;

        isInBetweenStates = false;

    }

    public bool GetIsCurrentlyInteractable()
    {
        return !isInClientUse && !isInBetweenStates;
    }
}

