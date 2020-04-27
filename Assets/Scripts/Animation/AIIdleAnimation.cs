using UnityEngine;
using UnityEngine.AI;

public class AIIdleAnimation : MonoBehaviour
{
    [SerializeField] float minTimeBetweenAnim = 3f;
    [SerializeField] float maxTimeBetweenAnim = 7f;
    //[SerializeField] string[] triggersNames;
    [SerializeField] float maxRandomDestinationDistance = 5f;

    private AnimatorManager m_AnimatorManager;
    private NavMeshAgent m_NavMeshAgent;


    private float timer = 0;
    private float currentTimerMax;

    private bool isOnWayToDest = false;
    void Awake()
    {
        m_AnimatorManager = GetComponent<AnimatorManager>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();

        currentTimerMax = Random.Range(minTimeBetweenAnim, maxTimeBetweenAnim);
    }

    void LateUpdate()
    {
        if (m_AnimatorManager.GetIsInIdle() && m_NavMeshAgent.isActiveAndEnabled)
        {
            // if is already walking somewhere
            if (isOnWayToDest)
            {
                CheckIfArrivedAtDest();

                // if arrived in destination and timer is not 0
                if (!isOnWayToDest)
                {
                    timer = 0;
                    return;
                }

                else
                {
                    return; // keep walking
                }
            }

            // else if is in idle animation (waiting)
            else
            {
                if (timer < currentTimerMax)
                {
                    timer += Time.deltaTime;
                }

                else
                {
                    // pick random destination
                    PickRandomPosAndSetDest();
                }
            }
        }

        else // if client is doing something, lets reset the timer
        {
            timer = 0;
        }
    }

    private void PickRandomPosAndSetDest()
    {
        Vector3 randomDestFromAgent = (Random.insideUnitSphere * maxRandomDestinationDistance) + transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDestFromAgent, out hit, maxRandomDestinationDistance, 1))
        {
            finalPosition = hit.position;
        }

        if (finalPosition != Vector3.zero)
        {
            m_NavMeshAgent.SetDestination(finalPosition);
            isOnWayToDest = true;
        }
        else
        {
            Debug.LogError("Check out what is going on with " + name);
        }
    }

    private void CheckIfArrivedAtDest()
    {
        if (!m_NavMeshAgent.pathPending)
        {
            if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                if (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    isOnWayToDest = false;
                }
            }
        }
    }
}
