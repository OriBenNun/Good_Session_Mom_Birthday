using UnityEngine;
using UnityEngine.AI;

public class ClickToInteract : MonoBehaviour
{
    [Header("SphereCast settings")]
    [SerializeField] float sphereCastRadius = 1f;


    NavMeshAgent myNavMeshAgent;
    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetDestinationToTapPosition();
        }
    }

    void SetDestinationToTapPosition()
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
                    if (interactable.GetInteractType() == InteractType.Move)
                    {
                        myNavMeshAgent.SetDestination(hits[i].point);
                    }
                }
            }
        }
    }
}
