using UnityEngine;
using UnityEngine.AI;

public class ClickToInteract : MonoBehaviour
{
    [Header("SphereCast settings")]
    [SerializeField] float tapSphereCastRadius = .2f;
    [SerializeField] float checkForObjectSphereCastRadius = .2f;

    [Header("Player Interaction Settings")]
    [SerializeField] private float rotationToInteractSpeed = 5f;

    [Header("Camera Settings")]
    [SerializeField] float minZoomFOV = 20f;
    [SerializeField] float maxZoomFOV = 105f;
    [SerializeField] float scrollWheelZoomFactor = 100;

    [SerializeField] private float zoomRequested = 50;
    [SerializeField] float zoomSpeed = 10;

    [SerializeField] float camMoveSpeed = 10;
    [SerializeField] Vector3 cameraOffsetFromPlayer;

    private IInteractable currentInteractingWith = null;
    private Vector3 currentInteractDest;

    private Vector3 newCamPos;
    private Vector3 camStartPos;

    private NavMeshAgent mNavMeshAgent;

    private bool onWayToInteractDest = false;
    private bool isRotatingToInteract = false;

    private void Awake()
    {
        mNavMeshAgent = GetComponent<NavMeshAgent>();
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

        if (isRotatingToInteract)
        {
            var targetRotation = Quaternion.LookRotation(currentInteractDest - transform.position);

            //Debug.Log("target " + targetRotation + " tansform " + transform.rotation);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, checkForObjectSphereCastRadius, transform.forward, out hit))
            {
                var temp = hit.transform.GetComponent<IInteractable>();
                if (temp == currentInteractingWith)
                {
                    Debug.Log("here2");
                    OnReachedDest();
                    isRotatingToInteract = false;
                }
            }

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationToInteractSpeed * Time.deltaTime);
        } // rotates the player to pick up item that within the stoppingdistance range

        else if (onWayToInteractDest)
        {
            UpdateDest();

            // Check if we've reached the destination
            if (!mNavMeshAgent.pathPending)
            {
                if (mNavMeshAgent.remainingDistance <= mNavMeshAgent.stoppingDistance)
                {
                    if (!mNavMeshAgent.hasPath || mNavMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        OnReachedDest();
                    }
                }
            }
        } // The checking if we should interact with the destination. Interaction get called here
        
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //Debug.Log("herer");
            Zoom(Input.GetAxis("Mouse ScrollWheel") * scrollWheelZoomFactor);
        }  // for PC / debugging

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
        // Multiply the offset x and z by the camera position, so the offset wil be in the opposite direction
        float xMultiplier = 1;
        float zMultiplier = 1;
        if (transform.position.x > 0) // checks if the camera is in the left side of the map
        {
            xMultiplier = -1;
        }

        if (transform.position.z > 0) // checks if the camera is in the bottom side of the map
        {
            zMultiplier = -1;
        }

        if (cameraOffsetFromPlayer.x < 0 && xMultiplier < 0)
        {
            // nothing because they are both negative
        }
        else
        {
            //cameraOffsetFromPlayer.x *= xMultiplier;
        }

        if (cameraOffsetFromPlayer.x < 0 && xMultiplier < 0)
        {
            // nothing because they are both negative
        }
        else
        {
            //cameraOffsetFromPlayer.z *= zMultiplier;
        }

        var fixedOffset = new Vector3(cameraOffsetFromPlayer.x * xMultiplier, 0, cameraOffsetFromPlayer.z * zMultiplier);

        // Calculates the relative position to the player by the fraction of the zoom
        // the more you zoom - the closer you get.
        float fraction = (1 - (zoomRequested / maxZoomFOV));

        var temp = (transform.position - camStartPos) * fraction - (fixedOffset * fraction);

        newCamPos = new Vector3(camStartPos.x + temp.x, camStartPos.y, camStartPos.z + temp.z);

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newCamPos, camMoveSpeed * Time.deltaTime);

    }

    private void LateUpdate()
    {

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomRequested, zoomSpeed * Time.deltaTime);

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
        mNavMeshAgent.SetDestination(currentInteractDest);
        //}
    }

    void InteractWithTapPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.SphereCastAll(ray, tapSphereCastRadius);
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
                            < mNavMeshAgent.stoppingDistance)
                        {
                            if (interactable != PlayerManager.instance.GetIInteractableHeld()) // to fix a bug where you look at a object you hold
                            {
                                isRotatingToInteract = true;
                                //transform.LookAt(interactable.GetInteractionPoint()); // TODO replace with rotation animation with root animation
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
                        mNavMeshAgent.SetDestination(hits[i].point);
                    }
                }
            }
        }
    }
}
