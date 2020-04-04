using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsAISystem : MonoBehaviour , IInteractable
{
    [SerializeField] Transform interactionPoint = null;
    [SerializeField] List<Need> needsList = null;

    [SerializeField] float minTimeBetweenNeeds = 3.5f;
    [SerializeField] float maxTimeBetweenNeeds = 7f;

    private NeedsIndicator needsIndicator;

    private Need currentNeed = null;
    private List<Need> usedNeeds;

    private bool isInCD = false;

    private void Awake()
    {
        //InitializeNeedsList();
    }

    private void Start()
    {
        needsIndicator = GetComponent<NeedsIndicator>();

        usedNeeds = new List<Need>();
        if (needsList.Count < 1) { Debug.LogError("this client " + name + " have no needs!"); return; }

        StartCoroutine("changeNeedSequence");
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
                if (UnityEngine.Random.Range(0, 100) > 70)
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
        return UnityEngine.Random.Range(0, needsList.Count);
    }

    public void OnInteraction()
    {
        if (!isInCD)
        {
            var playerHeld = PlayerManager.instance.GetIInteractableHeld();
            if (playerHeld != null)
            {
                Debug.Log(playerHeld.GetInteractableNeedsType() + " @@@@ " + currentNeed.GetNeedsType());
                if (playerHeld.GetInteractableNeedsType() == currentNeed.GetNeedsType())
                {
                    Debug.Log("YAY!");
                    StartCoroutine("changeNeedSequence");
                }
            }
        }
    }

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint.position;
    }

    IEnumerator changeNeedSequence()
    {
        isInCD = true;
        needsIndicator.DestroyNeedIndication();
        yield return new WaitForSeconds(RandomTimeBetweenNeeds());

        PickRandomNeed();
        needsIndicator.CreateNeedIndicator(currentNeed.popUpObject);
        isInCD = false;
    }

    private float RandomTimeBetweenNeeds()
    {
        return Random.Range(minTimeBetweenNeeds, maxTimeBetweenNeeds);
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

    public Need GetCurrentNeed()
    {
        return currentNeed;
    }
}
