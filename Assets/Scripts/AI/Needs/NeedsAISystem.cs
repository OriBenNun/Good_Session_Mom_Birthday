using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsAISystem : MonoBehaviour , IInteractable
{
    [SerializeField] Transform interactionPoint = null;

    [SerializeField] float minTimeBetweenNeeds = 3.5f;
    [SerializeField] float maxTimeBetweenNeeds = 7f;

    [SerializeField] int minFulfilledAnimationTime = 10;
    [SerializeField] int maxFulfilledAnimationTime = 20;

    [SerializeField] private Transform bigBallStartPoint = null;
    [SerializeField] private Transform smallBallStartPoint = null;
    [SerializeField] private Transform teddyStartPoint = null;

    private List<Need> needsList;

    private NeedsIndicator needsIndicator;

    private AnimatorManager mAnimatorManager;

    private DissolveMaterialCreatorController dissolver;

    private Need currentNeed = null;
    private List<Need> usedNeeds;

    private bool isInCD = false;
    private bool isInNeed = false;

    private bool playerHasFinishedGiveItemAnimation = false;
    private bool ableToReposition = false;

    private bool isHoldingPlayerHand = false;

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
        SetNeedsListFromArray(GameManager.instance.GetCurrentLevelNeddsArr()); // asking the game manager for the current level needs
        if (needsList.Count < 1) { Debug.LogError("this client " + name + " have no needs!"); return; }

        StartCoroutine("changeNeedSequence");

    }

    public void SetNeedsListFromArray(Need[] needsArray)
    {
        needsList = new List<Need>();
        for (int i = 0; i < needsArray.Length; i++)
        {
            needsList.Add(needsArray[i]);
        }
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

    public Transform GetStartAnimationPosition(NeedsType type)
    {
        if (type == NeedsType.BigBall)
        {
            return bigBallStartPoint;
        }

        if (type == NeedsType.SmallBall)
        {
            return smallBallStartPoint;
        }

        if (type == NeedsType.Teddy)
        {
            return teddyStartPoint;
        }

        else
        {
            return null;
            Debug.LogError("YEA YEA YEA" + name) ;
        }
    }

    public void OnInteraction()
    {
        if (!isInCD && isInNeed)
        {
            var playerHeld = PlayerManager.instance.GetIInteractableHeld();

            if (currentNeed.needObject)
            {
                if (playerHeld != null)
                {
                    if (playerHeld.GetInteractableNeedsType() == currentNeed.GetNeedsType())
                    {
                        StartCoroutine("FulfilledRecieveObjectNeedSequence", playerHeld);
                    }
                }
            }
            
            else if (!currentNeed.needObject)
            {
                if (playerHeld == null)
                {
                    if (!isHoldingPlayerHand)
                    {
                        HoldPlayerHand();
                    }

                    else
                    {
                        StopHoldPlayerHand();
                    }
                }
                else
                {
                    // Todo fail SFX
                    Debug.Log("First empty your hands");
                }
            }
        }
    }

    private void StopHoldPlayerHand()
    {
        StartCoroutine("StopHoldPlayerHandRoutine");

        isHoldingPlayerHand = false;

        PlayerManager.instance.isHoldingClientHand = false;
        PlayerManager.instance.currentlyHoldingClient = null;
    }

    public void HoldPlayerHand()
    {
        isHoldingPlayerHand = true;

        PlayerManager.instance.isHoldingClientHand = true;
        PlayerManager.instance.currentlyHoldingClient = this;

        StartCoroutine("StartHoldPlayerHandRoutine");
    }

    private IEnumerator StartHoldPlayerHandRoutine()
    {
        // Disable controller on player
        PlayerManager.instance.isPlayerAbleToControl = false;

        PlayerAnimatorController playerAnimator = PlayerManager.instance.GetPlayerAnimatorController();

        // todo move player to the right position and rotate to the child

        // turn on kinematics on both
        playerAnimator.ToggleNavAndKinematic(true);
        mAnimatorManager.ToggleNavAndKinematic(true);

        // play give hand animation on both
        playerAnimator.PlayTriggerAnimationSync("takeChildHand");
        mAnimatorManager.PlayTriggerAnimationSync("takePlayerHand");

        AnimationSyncManager.instance.PlaySyncTrigger();

        // wait until end of animation
        yield return new WaitForSeconds(0.5f); // to let the animation to end without headache

        // fade out both
        this.FadeObject(true);
        PlayerManager.instance.FadeObject(true);

        // hide the indicator with a bit of dely, for astetics sake
        yield return new WaitForSeconds(.5f);

        needsIndicator.HideIndicator(true);

        // wait for dissolve to end
        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool

        // reposition the client as a child in an empty object in player, so they will move together.
        transform.position = PlayerManager.instance.GetChildHoldingHandsTransform().position;
        transform.parent = PlayerManager.instance.GetChildHoldingHandsTransform();
        transform.rotation = PlayerManager.instance.GetChildHoldingHandsTransform().rotation;

        // make the player's nav mesh agent big, so both will be inside?

        // move the player to the holding hands blend tree animation
        playerAnimator.SetHoldingTypeAnimationState(HoldingObjectType.Client);
        mAnimatorManager.SetHoldingHandsAnimationBlend(true);

        // fade in both
        this.FadeObject(false);
        PlayerManager.instance.FadeObject(false);

        // show the indicator with a bit of dely, for astetics sake
        yield return new WaitForSeconds(.5f);

        needsIndicator.HideIndicator(false);

        // turn on nav mesh and rigidbody
        playerAnimator.ToggleNavAndKinematic(false);

        // Enable controller on player
        PlayerManager.instance.isPlayerAbleToControl = true;

    }

    private IEnumerator StopHoldPlayerHandRoutine()
    {
        // Disable controller on player
        PlayerManager.instance.isPlayerAbleToControl = false;

        PlayerAnimatorController playerAnimator = PlayerManager.instance.GetPlayerAnimatorController();

        // todo move player to the right position and rotate to the child

        // turn on kinematics on both
        playerAnimator.ToggleNavAndKinematic(true);
        mAnimatorManager.ToggleNavAndKinematic(true);

        // fade out both
        this.FadeObject(true);
        PlayerManager.instance.FadeObject(true);

        // hide the indicator with a bit of dely, for astetics sake
        yield return new WaitForSeconds(.5f);

        needsIndicator.HideIndicator(true);

        // wait for dissolve to end
        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool

        // set the child parent back to the Childs transform
        GameManager.instance.SetGameObjectAsClientsChildren(transform);

        // free the player and child from the holding hands blend tree animation
        playerAnimator.SetHoldingTypeAnimationState(HoldingObjectType.None);
        playerAnimator.ResetAnimToLoco();

        mAnimatorManager.SetHoldingHandsAnimationBlend(false);
        mAnimatorManager.TriggerAnimationNoSync("resetToLoco");

        // fade in both
        this.FadeObject(false);
        PlayerManager.instance.FadeObject(false);

        // show the indicator with a bit of dely, for astetics sake
        yield return new WaitForSeconds(.5f);

        needsIndicator.HideIndicator(false);

        // turn on nav mesh and rigidbody
        playerAnimator.ToggleNavAndKinematic(false);
        mAnimatorManager.ToggleNavAndKinematic(false);

        // Enable controller on player
        PlayerManager.instance.isPlayerAbleToControl = true;

    }


    private IEnumerator FulfilledRecieveObjectNeedSequence(IInteractable playerHeld)
    {
        isInNeed = false;

        // destroy the current need indicator
        needsIndicator.DestroyNeedIndication();
        // Todo add Ui VFX to the destroied indicator

        // stops current animation
        mAnimatorManager.StopAnimator();

        // disable nav mesh and colliders
        mAnimatorManager.ToggleNavAndKinematic(true);

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

        // enable nav mesh and colliders
        mAnimatorManager.ToggleNavAndKinematic(false);

        StartCoroutine("changeNeedSequence");
    }

    public IEnumerator FulfilledStaticToyNeedSequence(IInteractable staticToy, Transform startAnimPos)
    {
        isInNeed = false;

        // destroy the current need indicator
        needsIndicator.DestroyNeedIndication();
        // Todo add Ui VFX to the destroied indicator

        // stops current animation
        mAnimatorManager.StopAnimator();

        PlayerManager.instance.SuccesfulClientNeedFulfilled(); // Todo add effects to player and add to score and shit


        #region Free the player, turn HoldPlayerHand to false and and fade the child out

        // Disable controller on player
        PlayerManager.instance.isPlayerAbleToControl = false;

        PlayerAnimatorController playerAnimator = PlayerManager.instance.GetPlayerAnimatorController();

        // todo move player to the right position and rotate to the child

        // turn on kinematics on both
        playerAnimator.ToggleNavAndKinematic(true);
        mAnimatorManager.ToggleNavAndKinematic(true);

        // fade out both
        this.FadeObject(true);
        PlayerManager.instance.FadeObject(true);

        // wait for dissolve to end
        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool

        // set the child parent back to the Childs transform
        GameManager.instance.SetGameObjectAsClientsChildren(transform);

        // free the player and child from the holding hands blend tree animation
        playerAnimator.SetHoldingTypeAnimationState(HoldingObjectType.None);
        playerAnimator.ResetAnimToLoco();

        mAnimatorManager.SetHoldingHandsAnimationBlend(false);
        mAnimatorManager.TriggerAnimationNoSync("resetToLoco");

        // fade in Player
        PlayerManager.instance.FadeObject(false);

        // turn on nav mesh and rigidbody on player.
        playerAnimator.ToggleNavAndKinematic(false);

        isHoldingPlayerHand = false;

        PlayerManager.instance.isHoldingClientHand = false;
        PlayerManager.instance.currentlyHoldingClient = null;

        // Enable controller on player
        PlayerManager.instance.isPlayerAbleToControl = true;

        #endregion

        #region Start play with static toy sequence
        // Stores the original position to restore later
        var originalPos = transform.position;

        // Stores the original rotation to restore later if 
        //var originalRot = transform.rotation;

        // move the client to the Toy's start pos
        transform.position = startAnimPos.position;

        if (staticToy.GetInteractableNeedsType() == NeedsType.Tent)
        {
            transform.rotation = startAnimPos.rotation;
        }

        // random time to repeat animation
        var animationTime = UnityEngine.Random.Range(minFulfilledAnimationTime, maxFulfilledAnimationTime);

        // the static toy singed the trigger animation before this routine was called

        // sign the trigger to be synced
        mAnimatorManager.PlayTriggerAnimationSync(currentNeed.startAnimationTrigger);

        // restarts the animator
        mAnimatorManager.StartAnimator();

        // play sync animation on both
        AnimationSyncManager.instance.PlaySyncTrigger();

        // fade in client
        this.FadeObject(false);

        #endregion

        // wait for the loops to end
        yield return new WaitForSeconds(animationTime);

        #region Finish play with static toy sequence
        // fade out client
        this.FadeObject(true);

        // Stop the static toy animation
        staticToy.FadeObject(true); // using that as the stop animation function.. i know its shit, but i want to get over with it!!!!

        mAnimatorManager.TriggerAnimationNoSync(currentNeed.finishAnimationTrigger);

        // wait for fade out to finish
        yield return new WaitUntil(() => ableToReposition); // Happening in OnFinishedDissolveEvent, callback by the dissolver

        ableToReposition = false; // resets the bool

        // Restore the client's position and toggle kinematics back
        mAnimatorManager.ToggleKinematicAndMoveToPosition(originalPos, false);

        // fade in client
        this.FadeObject(false, 0.5f);

        // reset the InClientUse of StaticToy.. its shit. i know. if passing false thats what happening.
        staticToy.FadeObject(false);

        #endregion

        StartCoroutine("changeNeedSequence");
    } // Being called by the static toy


    private IEnumerator changeNeedSequence()
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

    public void FadeObject(bool shouldDissolve, float speed = 1)
    {
        if (dissolver == null) { Debug.LogError("Hey! missing a dissolver on " + name); return; }

        if (shouldDissolve && dissolver.GetIsVisible())
        {
            dissolver.StartDissolve(speed);
            dissolver.OnFinishedDissolve += OnFinishedDissolveEvent;
        }

        else if (!shouldDissolve && !dissolver.GetIsVisible())
        {
            dissolver.StartReverseDissolve(speed);
        }
    }

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint.position;
    }

    public InteractType GetInteractType()
    {
        return InteractType.Client;
    }

    public GameObject GetInteractableGameObject()
    {
        return gameObject;
    }

    public NeedsType GetInteractableNeedsType()
    {
        return currentNeed.GetNeedsType();
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
