using System.Collections;
using UnityEngine;

public class BallInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] NeedsType mNeedsType = NeedsType.BigBall;
    [SerializeField] HoldingObjectType mHoldingObjectType = HoldingObjectType.None;
    [SerializeField] float applyForceWhenDropped = 2;

    private bool isPickedUp = false;

    private bool isInClientUse = false;
    private bool isInBetweenStates = false; // another bool to squish some bugs when player is spam clicking the ball at the end of animation
    private bool ableToReposition = false;

    DissolveMaterialCreatorController dissolver;
    AnimatorManager mAnimator;


    private const string startTriggerString = "start_PlayWithChild";

    private const string finishTriggerString = "finish_PlayWithChild";

    private void Awake()
    {
        dissolver = GetComponentInChildren<DissolveMaterialCreatorController>();
        mAnimator = GetComponent<AnimatorManager>();
    }

    public InteractType GetInteractType()
    {
        return InteractType.PickableToy;
    }

    public Vector3 GetInteractionPoint()
    {
        return transform.position;
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
        mAnimator.ApplyRandomForce(applyForceWhenDropped);

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
