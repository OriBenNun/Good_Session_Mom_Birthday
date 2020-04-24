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

    [SerializeField] private Transform childTrampolineStartPoint = null;
    [SerializeField] private bool isGotBlendTrees = false;
    [SerializeField] float blendSmoothFactor = 7f;


    private string currentTriggerString = null;

    private bool isHoldingHands = false;

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

    void Update()
    {
        if (isGotBlendTrees)
        {
            UpdateAnimationBlendTrees();
        }
    }

    private void UpdateAnimationBlendTrees()
    {
        if (!isHoldingHands)
        {
            // sets the locomotion blend float for animator
            mChildrenAnimator.SetFloat("locomotionBlend", Mathf.Lerp(0, 1, mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
        } // talking with the locomotion blendtree

        else
        {
            // sets the holding Big blend float for animator
            mChildrenAnimator.SetFloat("holdingHandBlend", Mathf.Lerp(0, 1, PlayerManager.instance.GetPlayerAnimatorController().AnimationMovementSpeed()));
        } // talking with the HoldingBig blendtree

    } // sets the float as the velocity formula for the right blend tree

    public void SetHoldingHandsAnimationBlend(bool isHoldingHandsToggle)
    {
        isHoldingHands = isHoldingHandsToggle;
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

    public Vector3 GetTrampolineStartPoint()
    {
        return childTrampolineStartPoint.position;
    }

    public void StopAnimator()
    {
        mChildrenAnimator.speed = 0;
    }

    public void StartAnimator()
    {
        mChildrenAnimator.speed = 1;
    }

    public void ApplyRandomForce(float strength = 1)
    {
        mRigidBody.AddRelativeForce(new Vector3
            (Random.Range(10f, 20f), Random.Range(20f, 30f), Random.Range(10f, 20f)) * strength); // adds some random force for fun
    }
}
