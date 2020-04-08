using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] float blendSmoothFactor = 1f;


    private NavMeshAgent mNavMeshAgent;
    private Animator mAnimator;

    private void Awake()
    {
        mNavMeshAgent = GetComponent<NavMeshAgent>();
        mAnimator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        //mAnimator.SetFloat("motionBlend", Mathf.Clamp01(mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
        mAnimator.SetFloat("motionBlend", Mathf.Lerp(0, 1, mNavMeshAgent.velocity.sqrMagnitude * Time.deltaTime * blendSmoothFactor));
    }
}
