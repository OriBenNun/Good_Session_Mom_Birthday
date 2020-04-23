using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsAISystem : MonoBehaviour , IInteractable
{
    [SerializeField] Transform interactionPoint = null;
    [SerializeField] List<Need> needsList = null;

    [SerializeField] float minTimeBetweenNeeds = 3.5f;
    [SerializeField] float maxTimeBetweenNeeds = 7f;

    [SerializeField] int minFulfilledAnimationTime = 10;
    [SerializeField] int maxFulfilledAnimationTime = 20;

    private NeedsIndicator needsIndicator;

    private AnimatorManager mAnimatorManager;

    private Need currentNeed = null;
    private List<Need> usedNeeds;

    private bool isInCD = false;
    private bool isInNeed = false;

    private bool playerHasFinishedGiveItemAnimation = false;
    private bool ableToReposition = false;

    DissolveMaterialCreatorController dissolver;
    private void Awake()
    {
        dissolver = GetComponentInChildren<DissolveMaterialCreatorController>();
        mAnimatorManager = GetComponent<AnimatorManager>();
    }

    private void OnDestroy()
    {
        PlayerManager.instance.OnPlayerFinishedGiveAnimation -= PlayerFinishedGiveItemAnimation;
    }

    private void Start()
    {
        PlayerManager.instance.OnPlayerFinishedGiveAnimation += PlayerFinishedGiveItemAnimation; // singing here instead of OnEnable because of condition race, player is not ready on OnEnable.

        needsIndicator = GetComponent<NeedsIndicator>();

        usedNeeds = new List<Need>();
        if (needsList.Count < 1) { Debug.LogError("this client " + name + " have no needs!"); return; }

        StartCoroutine("changeNeedSequence");

    }

    private void PickRandomNeed()
    {
        while (true)
        {
            int random = GenerateRandom();
            var newNeed = needsList[random];
            if (currentNeed == null) 
            {
                usedNeeds.Add(currentNeed);
                currentNeed = newNeed;
                break;
            }

            else if (needsList.Count > 1 && newNeed.GetNeedsType() == currentNeed.GetNeedsType()) 
            {
                continue;
            }

            if (usedNeeds.Contains(newNeed))
            {
                if (UnityEngine.Random.Range(0, 100) > 70)
                {
                    currentNeed = newNeed;
                    break;
                }
                else 
                {
                    continue; 
                }
            }

            else
            {
                usedNeeds.Add(currentNeed);
                currentNeed = newNeed;
                break;
            }
        }

        isInNeed = true;
    }

    private int GenerateRandom()
    {
        return UnityEngine.Random.Range(0, needsList.Count);
    }

    public Vector3 GetStartAnimationPosition(NeedsType needType)
    {
        if (needType == NeedsType.BigBall)
        {
            return mAnimatorManager.GetBigBallStartPoint();
        }

        if (needType == NeedsType.SmallBall)
        {
            return mAnimatorManager.GetSmallBallStartPoint();
        }
        return Vector3.zero;
    }

    public void OnInteraction()
    {
        if (!isInCD && isInNeed)
        {
            var playerHeld = PlayerManager.instance.GetIInteractableHeld();
            if (playerHeld != null)
            {
                //Debug.Log(playerHeld.GetInteractableNeedsType() + " @@@@ " + currentNeed.GetNeedsType());
                if (playerHeld.GetInteractableNeedsType() == currentNeed.GetNeedsType())
                {
                    //FulfilledRecieveObjectNeedSequence(playerHeld);
                    StartCoroutine("FulfilledRecieveObjectNeedSequence", playerHeld);
                }
            }
        }
    }

    private IEnumerator FulfilledRecieveObjectNeedSequence(IInteractable playerHeld)
    {
        isInNeed = false;

        // destroy the current need indicator
        needsIndicator.DestroyNeedIndication();
        // Todo add Ui VFX to the destroied indicator

        // stops current animation
        mAnimatorManager.StopAnimator();

        if (playerHasFinishedGiveItemAnimation) { playerHasFinishedGiveItemAnimation = false; } // Because it happens from a event, which means all clients get the callback
        
        PlayerManager.instance.SuccesfulClientNeedFulfilled(); // Todo add effects to player and add to score and shit

        // player give the object, animation and logical (the player get released from object and return to normal)
        StartCoroutine(playerHeld.OnFulfilledNeedBehaviour(this));

        // wait for player's animation to end
        yield return new WaitUntil(() => playerHasFinishedGiveItemAnimation);

        playerHasFinishedGiveItemAnimation = false; // resets the bool

        // fade out object
        playerHeld.FadeObject(true);

        // fade out client
        this.FadeObject(true);

        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool

        // reposition object happened on the object after FadeObject is called

        // random number of times to loop animation
        var animationTime = UnityEngine.Random.Range(minFulfilledAnimationTime, maxFulfilledAnimationTime);

        // disable nav mesh and colliders
        mAnimatorManager.ToggleNavAndKinematic(true);

        // play sync animation on both
        mAnimatorManager.PlayTriggerAnimationSync(currentNeed.startAnimationTrigger);

        AnimationSyncManager.instance.PlaySyncTrigger();

        // restarts the animator
        mAnimatorManager.StartAnimator();

        // fade in object
        playerHeld.FadeObject(false);

        // fade in client
        this.FadeObject(false);

        // wait for the loops to end
        yield return new WaitForSeconds(animationTime);

        // fade out object
        playerHeld.FadeObject(true);

        // fade out client
        this.FadeObject(true);

        // stop animator
/*        mAnimatorManager.PlayTriggerAnimationSync(currentNeed.finishAnimationTrigger);

        AnimationSyncManager.instance.PlaySyncTrigger();*/

        mAnimatorManager.TriggerAnimationNoSync(currentNeed.finishAnimationTrigger);

        // wait fo fade out to finish
        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool


        // reposition object and turn on nav mesh and colliders
        playerHeld.OnInteraction();

        // fade in object
        playerHeld.FadeObject(false, 0.5f);

        // fade in client
        this.FadeObject(false, 0.5f);

        StartCoroutine("changeNeedSequence");
    }

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint.position;
    }

    IEnumerator changeNeedSequence()
    {
        isInCD = true;
        yield return new WaitForSeconds(RandomTimeBetweenNeeds());

        PickRandomNeed();
        needsIndicator.CreateNeedIndicator(currentNeed.popUpObject);
        isInCD = false;
    }

    private float RandomTimeBetweenNeeds()
    {
        return UnityEngine.Random.Range(minTimeBetweenNeeds, maxTimeBetweenNeeds);
    }

    private void PlayerFinishedGiveItemAnimation()
    {
        playerHasFinishedGiveItemAnimation = true;
    }

    private void OnFinishedDissolveEvent()
    {
        ableToReposition = true;

        dissolver.OnFinishedDissolve -= OnFinishedDissolveEvent;
    }

    public void FadeObject(bool shouldFade, float speed = 1)
    {
        if (dissolver == null) { Debug.LogError("Hey! missing a dissolver on " + name); return; }

        if (shouldFade && dissolver.GetIsVisible())
        {
            dissolver.StartDissolve(speed);
            dissolver.OnFinishedDissolve += OnFinishedDissolveEvent;
        }

        else if (!shouldFade && !dissolver.GetIsVisible())
        {
            dissolver.StartReverseDissolve(speed);
        }
    }

    public InteractType GetInteractType()
    {
        return InteractType.Interact;
    }

    public GameObject GetInteractableGameObject()
    {
        return gameObject;
    }

    public NeedsType GetInteractableNeedsType()
    {
        return NeedsType.None;
    }

    public Need GetCurrentNeed()
    {
        return currentNeed;
    }

    public HoldingObjectType GetHoldingObjectType()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator OnFulfilledNeedBehaviour(NeedsAISystem client)
    {
        throw new System.NotImplementedException();
    }

    public bool GetIsCurrentlyInteractable()
    {
        return isInNeed && !isInCD;
    }
}
