using UnityEngine;
using UnityEngine.AI;

public class ClickToInteract : MonoBehaviour
{
    [Header("SphereCast settings")]
    [SerializeField] float sphereCastRadius = 1f;

    //[SerializeField] float distThresholdToUpdateDest = 0.5f;


    NavMeshAgent myNavMeshAgent;
    private bool onWayToInteractDest = false;
    private IInteractable currentInteractingWith = null;
    private Vector3 currentInteractDest;

    [SerializeField] float minZoomFOV = 20f;
    [SerializeField] float maxZoomFOV = 105f;
    [SerializeField] float scrollWheelZoomFactor = 100;

    [SerializeField] private float zoomRequested = 50;
    [SerializeField] float zoomSpeed = 10;
    private Vector3 newCamPos;
    private Vector3 camStartPos;
    [SerializeField] float camMoveSpeed = 10;
    [SerializeField] Vector3 cameraOffsetFromPlayer;

    private void Awake()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();
        camStartPos = Camera.main.transform.position;
        newCamPos = camStartPos;
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            PinchZoom();
        }  // checking for pinch to zoom first

        else if (Input.touchCount > 2)
        {
            return;
        } // TODO some visual explaination, no more than 2 fingers commands

        else if (Input.GetMouseButtonDown(0))
        {
            InteractWithTapPosition();
        }  // the main tap handler

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //Debug.Log("herer");
            Zoom(Input.GetAxis("Mouse ScrollWheel") * scrollWheelZoomFactor);
        }  // for PC / debugging

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
        } // The checking if we should interact with the destination. Interaction get called here

        CameraPosUpdater(); // Updating the position of the camera according to the player

    }

    private void PinchZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        Zoom(difference * 0.05f);
    }

    private void CameraPosUpdater()
    {
        float fraction = (1 - (zoomRequested / maxZoomFOV));
        var temp = (transform.position - camStartPos) * fraction - (cameraOffsetFromPlayer * fraction);

        newCamPos = new Vector3(camStartPos.x + temp.x, camStartPos.y, camStartPos.z + temp.z);
    }

    private void LateUpdate()
    {

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomRequested, zoomSpeed * Time.deltaTime);

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newCamPos, camMoveSpeed * Time.deltaTime);

    }

    private void Zoom(float increment)
    {
        float amount = Camera.main.fieldOfView - increment;
        zoomRequested = Mathf.Clamp(amount, minZoomFOV, maxZoomFOV);
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
                            if (interactable != PlayerManager.instance.GetIInteractableHeld()) // to fix a bug where you look at a object you hold
                            {
                                transform.LookAt(interactable.GetInteractionPoint()); // TODO replace with rotation animation with root animation
                            }
                        }

                        currentInteractingWith = interactable;
                        currentInteractDest = currentInteractingWith.GetInteractionPoint();
                        onWayToInteractDest = true;
                        return;
                    }

                    else if (interactable.GetInteractType() == InteractType.Move)
                    {
                        if (onWayToInteractDest) { onWayToInteractDest = false; } // if the player currenty going somewhere but changing his mind and want to walk away
                        myNavMeshAgent.SetDestination(hits[i].point);
                    }
                }
            }
        }
    }
}
