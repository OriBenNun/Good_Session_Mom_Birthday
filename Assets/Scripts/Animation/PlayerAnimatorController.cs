using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] float blendSmoothFactor = 1f;


    private NavMeshAgent mNavMeshAgent;
    private Animator mAnimator;

    private HoldingObjectType mHoldingObjectType = HoldingObjectType.None;
    private void Awake()
    {
        mNavMeshAgent = GetComponent<NavMeshAgent>();
        mAnimator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        UpdateAnimationBlendTrees();
    }

    private void UpdateAnimationBlendTrees()
    {
        if (mHoldingObjectType == HoldingObjectType.None)
        {
            // sets the locomotion blend float for animator
            mAnimator.SetFloat("locomotionBlend", Mathf.Lerp(0, 1, mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
        } // talking with the locomotion blendtree

        else if (mHoldingObjectType == HoldingObjectType.Big)
        {
            // sets the holding Big blend float for animator
            mAnimator.SetFloat("holdingBigBlend", Mathf.Lerp(0, 1, mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
        } // talking with the HoldingBig blendtree

        else if (mHoldingObjectType == HoldingObjectType.Small)
        {
            // sets the holding Small blend float for animator
            mAnimator.SetFloat("holdingSmallBlend", Mathf.Lerp(0, 1, mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
        } // talking with the HoldingSmall blendtree
    } // sets the float as the velocity formula for the right blend tree

    public void SetHoldingTypeAnimationState(HoldingObjectType type)
    {
        mHoldingObjectType = type;
    }

    public void PickupAnimation(HoldingObjectType type)
    {
        if (type == HoldingObjectType.Big)
        {
            mAnimator.SetTrigger("pickingUp_Big");
        }

        else if (type == HoldingObjectType.Small)
        {
            mAnimator.SetTrigger("pickingUp_Small");
        }

        SetHoldingTypeAnimationState(type);
    }

    public void ResetAnimToLoco()
    {
        SetHoldingTypeAnimationState(HoldingObjectType.None);
        mAnimator.SetTrigger("backToLoco");
    }

    public void GiveItemTrigger()
    {
        mAnimator.SetTrigger("giveItem");
    }
}