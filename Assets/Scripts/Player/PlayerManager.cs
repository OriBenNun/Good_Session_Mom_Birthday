using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance; // Singelton


    [Header("Picking Up Settings")]
    [SerializeField] GrabbingPoint grabbingPoint = null;
    [SerializeField] Transform interactablesParent = null;


    private bool isHoldingSomething = false;
    private IInteractable currentObjectHold = null;

    private void Awake()
    {
        if (PlayerManager.instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public GameObject GetPlayerGameObject()
    {
        return this.gameObject;
    }

    public void PickUpObject(GameObject interactable)
    {
        if (isHoldingSomething)
        {
            currentObjectHold.OnInteraction(); // tells the current held object to get dropped
        }

        if (!isHoldingSomething)
        {
            isHoldingSomething = true;
            interactable.transform.position = grabbingPoint.transform.position;
            interactable.transform.parent = grabbingPoint.transform;
            currentObjectHold = interactable.GetComponent<IInteractable>();
        }
    }

    public void DropObject()
    {
        if (currentObjectHold == null) { return; }
        isHoldingSomething = false;
        currentObjectHold.GetInteractableGameObject().transform.parent = interactablesParent;
        currentObjectHold = null;
    }
}
