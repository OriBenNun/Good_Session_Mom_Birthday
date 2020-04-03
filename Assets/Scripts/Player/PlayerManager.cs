using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance; // Singelton


    [Header("Picking Up Settings")]
    [SerializeField] GrabbingPoint grabbingPoint = null;
    [SerializeField] Transform interactablesParent = null;

    [Header("Player's Settings")]
    [SerializeField] float doublebackExitTimer = 1f;
    private bool isHoldingSomething = false;
    private IInteractable currentInteractableHold = null;

    private float exitTimer = Mathf.Infinity;


    private void Awake()
    {
        if (PlayerManager.instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        DoubleBackExitApp();
    }

    private void DoubleBackExitApp()
    {
        if (exitTimer < doublebackExitTimer)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitTimer = 0;
        }
        exitTimer += Time.deltaTime;
    }

    public GameObject GetPlayerGameObject()
    {
        return this.gameObject;
    }

    public void PickUpObject(GameObject interactable)
    {
        if (isHoldingSomething)
        {
            currentInteractableHold.OnInteraction(); // tells the current held object to get dropped
        }

        if (!isHoldingSomething)
        {
            isHoldingSomething = true;
            interactable.transform.position = grabbingPoint.transform.position;
            interactable.transform.parent = grabbingPoint.transform;
            currentInteractableHold = interactable.GetComponent<IInteractable>();
        }
    }

    public void DropObject()
    {
        if (currentInteractableHold == null) { return; }
        isHoldingSomething = false;
        currentInteractableHold.GetInteractableGameObject().transform.parent = interactablesParent;
        currentInteractableHold = null;
    }

    public IInteractable GetIInteractableHeld()
    {
        return currentInteractableHold;
    }
}
