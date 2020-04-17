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
            Debug.Log("game object " + name + " doesnt have any nav mesh agent");
        }

        mRigidBody = GetComponent<Rigidbody>();
        if (mRigidBody == null)
        {
            Debug.Log("game object " + name + " doesnt have any rigidbody");
        }
    }

    public void PlayTriggerAnimationSync(string triggerString)
    {
        if (string.IsNullOrWhiteSpace(triggerString)) { return; }
        currentTriggerString = triggerString;

        ToggleOffNavAndRigidBody(false);

        AnimationSyncManager.instance.onReadyToSyncTrigger += TriggerAnimation;
    }

    private void TriggerAnimation()
    {
        mChildrenAnimator.SetTrigger(currentTriggerString);
    }

    private void ToggleOffNavAndRigidBody(bool toggle)
    {
        if (mNavMeshAgent != null)
        {
            mNavMeshAgent.enabled = toggle;
        }

        if (mRigidBody != null)
        {
            mRigidBody.isKinematic = !toggle;
        }
    }

    public void FadeOutAndMoveToStartPosition(Vector3 position)
    {
        ToggleOffNavAndRigidBody(false);

        // fade out

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
}
