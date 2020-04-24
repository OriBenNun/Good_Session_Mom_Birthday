using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] float blendSmoothFactor = 7f;


    private NavMeshAgent mNavMeshAgent;
    private Animator mAnimator;
    private Rigidbody mRigidBody;

    private HoldingObjectType mHoldingObjectType = HoldingObjectType.None;

    private string currentTriggerString = null;

    private bool originalKinematics;

    private void Awake()
    {
        mNavMeshAgent = GetComponent<NavMeshAgent>();
        mAnimator = GetComponentInChildren<Animator>();
        mRigidBody = GetComponent<Rigidbody>();
        originalKinematics = mRigidBody.isKinematic;
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
            mAnimator.SetFloat("locomotionBlend", Mathf.Lerp(0, 1, AnimationMovementSpeed()));
        } // talking with the locomotion blendtree

        else if (mHoldingObjectType == HoldingObjectType.Big)
        {
            // sets the holding Big blend float for animator
            mAnimator.SetFloat("holdingBigBlend", Mathf.Lerp(0, 1, AnimationMovementSpeed()));
        } // talking with the HoldingBig blendtree

        else if (mHoldingObjectType == HoldingObjectType.Small)
        {
            // sets the holding Small blend float for animator
            mAnimator.SetFloat("holdingSmallBlend", Mathf.Lerp(0, 1, AnimationMovementSpeed()));
        } // talking with the HoldingSmall blendtree

        else if (mHoldingObjectType == HoldingObjectType.Client)
        {
            // sets the holding Small blend float for animator
            mAnimator.SetFloat("holdingChildHandBlend", Mathf.Lerp(0, 1, AnimationMovementSpeed()));
        } // talking with the HoldingChildHand blendtree

    } // sets the float as the velocity formula for the right blend tree

    public float AnimationMovementSpeed()
    {
        return mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor;
    }

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
            mAnimator.SetTrigger(currentTriggerString);
            currentTriggerString = null;
            AnimationSyncManager.instance.OnReadyToSyncTrigger -= TriggerAnimation;
        }
        else
        {
            Debug.LogError("Check out whats going on with CurrentTriggerString on " + name);
        }
    }

    public void ToggleNavAndKinematic(bool isKinematic)
    {
        if (mNavMeshAgent != null)
        {
            mNavMeshAgent.enabled = !isKinematic;
        }

        if (mRigidBody != null)
        {
            if (!isKinematic)
            {
                mRigidBody.isKinematic = originalKinematics;
            }
            else
            {
                mRigidBody.isKinematic = isKinematic;
            }
        }
    }

    public void StopAnimator()
    {
        mAnimator.speed = 0;
    }

    public void StartAnimator()
    {
        mAnimator.speed = 1;
    }
}