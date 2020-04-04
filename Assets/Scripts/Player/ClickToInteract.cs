using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickToInteract : MonoBehaviour
{
    [Header("SphereCast settings")]
    [SerializeField] float sphereCastRadius = 1f;

    //[SerializeField] float distThresholdToUpdateDest = 0.5f;


    NavMeshAgent myNavMeshAgent;
    private bool onWayToInteractDest = false;
    private IInteractable currentInteractingWith = null;
    private Vector3 currentInteractDest;
    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InteractWithTapPosition();
        }

        if (onWayToInteractDest)
        {
            UpdateDest();

            // Check if we've reached the destination
            if (!myNavMeshAgent.pathPending)
            {
                if (myNavMeshAgent.remainingDistance <= myNavMeshAgent.stoppingDistance)
                {
                    if (!myNavMeshAgent.hasPath || myNavMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        OnReachedDest();
                    }
                }
            }
        }
    }

    private void OnReachedDest()
    {
        onWayToInteractDest = false;
        currentInteractingWith.OnInteraction();
    }

    private void UpdateDest()
    {
        //if (Vector3.Distance(currentInteractDest, currentInteractingWith.GetInteractionPoint()) >= distThresholdToUpdateDest) // to prevent "kicking" bug, where the player collides with the object and therefore can never reach the destination
        //{
        //Debug.Log("here " + currentInteractingWith);
        currentInteractDest = currentInteractingWith.GetInteractionPoint();
        myNavMeshAgent.SetDestination(currentInteractDest);
        //}
    }

    void InteractWithTapPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereCastRadius);
        if (hits.Length > 0)
        {
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                var interactable = hits[i].transform.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.GetInteractType() == InteractType.Interact)
                    {
                        // Checking if the object is not our current object and it within the stopping distance of the navmesh agent, so we could turn around
                        if (interactable != currentInteractingWith &&
                            Vector3.Distance(transform.position, interactable.GetInteractionPoint())
                            < myNavMeshAgent.stoppingDistance)
                        {
                            transform.LookAt(interactable.GetInteractionPoint()); // TODO replace with rotation animation with root animation
                        }

                        currentInteractingWith = interactable;
                        currentInteractDest = currentInteractingWith.GetInteractionPoint();
                        onWayToInteractDest = true;
                        return;
                    }

                    else if (interactable.GetInteractType() == InteractType.Move)
                    {
                        myNavMeshAgent.SetDestination(hits[i].point);
                    }
                }
            }
        }
    }
}
