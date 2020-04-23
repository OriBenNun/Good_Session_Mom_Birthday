using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimatorManager : MonoBehaviour
{
    private Animator mChildrenAnimator;

    private NavMeshAgent mNavMeshAgent;

    private Rigidbody mRigidBody;

    [SerializeField] private Transform bigBallStartPoint = null;
    [SerializeField] private Transform smallBallStartPoint = null;

    private string currentTriggerString = null;

    private void Start()
    {
        mChildrenAnimator = GetComponentInChildren<Animator>();
        if (mChildrenAnimator == null)
        {
            Debug.LogError("Child " + name + " doesnt have any animator");
            Debug.Break();
        }

        mNavMeshAgent = GetComponent<NavMeshAgent>();
        if (mNavMeshAgent == null)
        {
            //Debug.Log("game object " + name + " doesnt have any nav mesh agent");
        }

        mRigidBody = GetComponent<Rigidbody>();
        if (mRigidBody == null)
        {
            //Debug.Log("game object " + name + " doesnt have any rigidbody");
        }
    }

    public void PlayTriggerAnimationSync(string triggerString)
    {
        if (string.IsNullOrWhiteSpace(triggerString)) { return; }

        currentTriggerString = triggerString;

        AnimationSyncManager.instance.OnReadyToSyncTrigger += TriggerAnimation;
    }

    private void TriggerAnimation()
    {
        if (!string.IsNullOrEmpty(currentTriggerString))
        {
            mChildrenAnimator.SetTrigger(currentTriggerString);
            currentTriggerString = null;
            AnimationSyncManager.instance.OnReadyToSyncTrigger -= TriggerAnimation;
        }
        else
        {
            Debug.LogError("Check out whats going on with CurrentTriggerString on " + name);
        }
    }

    public void TriggerAnimationNoSync(string trigger)
    {
        mChildrenAnimator.SetTrigger(trigger);
    }

    public void ToggleNavAndKinematic(bool isKinematic)
    {
        if (mNavMeshAgent != null)
        {
            mNavMeshAgent.enabled = !isKinematic;
        }

        if (mRigidBody != null)
        {
            mRigidBody.isKinematic = isKinematic;
        }
    }

    public void ToggleKinematicAndMoveToPosition(Vector3 position, bool isKinematic)
    {
        ToggleNavAndKinematic(isKinematic);

        this.transform.position = position;
        this.transform.rotation = Quaternion.identity;
    }

    public Vector3 GetBigBallStartPoint()
    {
        return bigBallStartPoint.position;
    }

    public Vector3 GetSmallBallStartPoint()
    {
        return smallBallStartPoint.position;
    }

    public void StopAnimator()
    {
        mChildrenAnimator.speed = 0;
    }

    public void StartAnimator()
    {
        mChildrenAnimator.speed = 1;
    }
/*
    public float GetCurrentAnimationLengthForLoops(int numOfLoops)
    {
        Debug.Log("Should wait for " + mChildrenAnimator.runtimeAnimatorController.animationClips[].length * numOfLoops + " which is " + numOfLoops + " loops");
        return mChildrenAnimator.GetCurrentAnimatorStateInfo(0).length * numOfLoops;
    }*/

    public void ApplyRandomForce(float strength = 1)
    {
        mRigidBody.AddRelativeForce(new Vector3
            (Random.Range(10f, 20f), Random.Range(20f, 30f), Random.Range(10f, 20f)) * strength); // adds some random force for fun
    }
}
