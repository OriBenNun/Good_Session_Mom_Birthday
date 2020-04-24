﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineInteractable : MonoBehaviour , IInteractable
{
    [SerializeField] NeedsType mNeedsType = NeedsType.Trampoline;
    [SerializeField] HoldingObjectType mHoldingObjectType = HoldingObjectType.Client;
    [SerializeField] Transform interactPoint = null;
    [SerializeField] private Transform childTrampolineStartPoint = null;

    private bool isInClientUse = false;

    AnimatorManager mAnimator;


    private const string startTriggerString = "start_ChildJumping";

    private const string finishTriggerString = "finish_ChildJumping";

    private void Awake()
    {
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

        if (PlayerManager.instance.isHoldingClientHand)
        {
            Debug.Log("Hey2");
            if (PlayerManager.instance.currentlyHoldingClient != null)
            {
                Debug.Log("Hey3");
                NeedsAISystem client = PlayerManager.instance.currentlyHoldingClient.GetInteractableGameObject().GetComponent<NeedsAISystem>();

                if (client.GetInteractableNeedsType() == mNeedsType)
                {
                    Debug.Log("Hey4");
                    isInClientUse = true;

                    mAnimator.PlayTriggerAnimationSync(startTriggerString);

                    StartCoroutine(client.FulfilledStaticToyNeedSequence(this, childTrampolineStartPoint.position));

                }
            }
        }

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
        throw new System.NotImplementedException();
    }

    public void FadeObject(bool shouldFade, float speed = 1) // using that as the stop animation function.. i know its shit, but i want to get over with it!!!!
    {
        // if true passed stops the animation:
        if (shouldFade)
        {
        mAnimator.TriggerAnimationNoSync(finishTriggerString);
        }
        else
        {
            isInClientUse = false;
        }
    }

    public bool GetIsCurrentlyInteractable()
    {
        throw new System.NotImplementedException();
    }
}

