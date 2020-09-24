using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Need", menuName = "Needs System")]
public class Need : ScriptableObject
{
    [SerializeField] NeedsType needsType;
    [SerializeField] public bool needObject = false;
    //[SerializeField] public GameObject interactablePrefab = null;
    [SerializeField] public string startAnimationTrigger = null;
    [SerializeField] public string finishAnimationTrigger = null;
    [SerializeField] public PopUpObject popUpObject = null;
    [SerializeField] public GameObject needPrefab = null;

/*    public IInteractable GetObjectInteractable()
    {
        var temp = interactablePrefab.GetComponent<IInteractable>();
        if (temp != null)
        {
            return temp;
        }
        else
        {
            Debug.LogError("need: " + this.name + "need fixing with the prefab. no IInteractable found");
            return null;
        }
    }*/

    public NeedsType GetNeedsType()
    {
        return needsType;
    }
}


public enum NeedsType
{
    None,
    BigBall,
    SmallBall,
    Trampoline,
    Tent,
    Teddy
}


public enum HoldingObjectType
{
    Big,
    Small,
    Client,
    None
}

