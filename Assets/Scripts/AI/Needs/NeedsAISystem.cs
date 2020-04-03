using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsAISystem : MonoBehaviour , IInteractable
{
    [SerializeField] Transform interactionPoint = null;
    [SerializeField] Need[] needsArray = null;

    private Need currentNeed = null;
    private List<Need> needsList;
    private List<Need> usedNeeds;

    private void Awake()
    {
        InitializeNeedsList();
    }

    private void InitializeNeedsList()
    {
        needsList = new List<Need>();
        foreach (Need need in needsArray)
        {
            needsList.Add(need);
        }
    }

    private void Start()
    {
        usedNeeds = new List<Need>();
        PickRandomNeed();
    }

    private void PickRandomNeed()
    {
        while (true)
        {
            int random = GenerateRandom();
            var newNeed = needsList[random];
            if (currentNeed == null) 
            {
                usedNeeds.Add(currentNeed);
                currentNeed = newNeed;
                break;
            }

            if (needsList.Count > 1 && newNeed.GetNeedsType() == currentNeed.GetNeedsType()) 
            {
                continue; 
            }

            if (usedNeeds.Contains(newNeed))
            {
                if (Random.Range(0, 100) > 70)
                {
                    currentNeed = newNeed;
                    break;
                }
                else 
                {
                    continue; 
                }
            }

            else
            {
                usedNeeds.Add(currentNeed);
                currentNeed = newNeed;
                break;
            }
        }
        Debug.Log("picked need: " + currentNeed.name);
    }

    private int GenerateRandom()
    {
        return Random.Range(0, needsList.Count);
    }

    public void OnInteraction()
    {
        var playerHeld = PlayerManager.instance.GetIInteractableHeld();
        if (playerHeld != null)
        {
            Debug.Log(playerHeld.GetInteractableNeedsType() + " @@@@ " + currentNeed.GetNeedsType());
            if (playerHeld.GetInteractableNeedsType() == currentNeed.GetNeedsType())
            {
                Debug.Log("YAY!");
                PickRandomNeed();
            }
        }
    }

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint.position;
    }

    public InteractType GetInteractType()
    {
        return InteractType.Interact;
    }

    public GameObject GetInteractableGameObject()
    {
        return gameObject;
    }

    public NeedsType GetInteractableNeedsType()
    {
        return NeedsType.None;
    }
}
