using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singelton

    [Header("Level Configs")]
    [SerializeField] int firstLevelNumber = 1;
    [SerializeField] LevelConfig[] levelsConfigs = null;
    [SerializeField] GameObject[] availableClients = null;

    [Header("Hirarchy Organise")]
    [SerializeField] Transform clientsParentTransform = null;

    [Header("General Gameplay Settings")]
    [SerializeField] float levelTimeLengthSeconds = 120;
    [SerializeField] Transform[] clientsSpawnPoints;

    [Header("Design Settings")]
    [SerializeField] float timeBetweenClientsSpawnInStartOfLevel = 1f;
    [SerializeField] float FadeInSpeedInStartOfLevel = .5f;

    private int currentLevelLoadedNumber = 0;
    private GameObject[] currentClientsInScene;
    private Need[] currentLevelNeedsArr; // for the client to ask once it spawned
    private void Awake()
    {
        if (GameManager.instance == null)
        {
            GameManager.instance = this;
        }  // Singelton
    }

    private void Start()
    {
        if (currentLevelLoadedNumber == 0)
        {
            StartCoroutine("LoadLevelFromConfig", firstLevelNumber);
        }
    }

    public void SetGameObjectAsClientsChildren(Transform t)
    {
        t.parent = clientsParentTransform;
    }

    private IEnumerator LoadLevelFromConfig(int levelNumberToLoad)
    {
        // check the level to load
        LevelConfig levelToLoad = null;
        foreach (var level in levelsConfigs)
        {
            if (level.levelNumber == levelNumberToLoad)
            {
                levelToLoad = level;
                currentLevelLoadedNumber = level.levelNumber;
                currentLevelNeedsArr = level.needsTypeInLevel;
            }
        }

        // saftry measures
        if (levelToLoad == null)
        {
            levelToLoad = levelsConfigs[0];
            Debug.LogError("Something is wrong with level loader on level number " + levelNumberToLoad + ". loading first level for now");
            Debug.Break();
        }

        // if first level - dissolve the player - currently the player's start invisible is false, can change in inspector on the dissolver script)

        #region Instatiate random clients by the number of clients in level

        // reset the currentClientsInScene for later use (to destroy at end of level)
        currentClientsInScene = new GameObject[levelToLoad.numberOfClients];
        
        // Making a list for the available client indexes, to not instatiate the same client twice and for diversity between levels
        GameObject[] clientsToLoad = new GameObject[levelToLoad.numberOfClients];
        List<int> availableClientsIndexes = new List<int>();
        for (int i = 0; i < availableClients.Length; i++)
        {
            availableClientsIndexes.Add(i);
        }

        // Handles the logic of the randomness
        for (int i = 0; i < levelToLoad.numberOfClients; i++)
        {
            int randomClientIndex = Random.Range(0, availableClientsIndexes.Count);

            availableClientsIndexes.RemoveAt(randomClientIndex);

            clientsToLoad[i] = availableClients[randomClientIndex];
        }

        // Instatiating the clients array that was formed in the randomness handler
        for (int i = 0; i < clientsToLoad.Length; i++)
        {
            var newChild = Instantiate(clientsToLoad[i], clientsSpawnPoints[i].position, Quaternion.identity, clientsParentTransform);
            currentClientsInScene[i] = newChild;

            yield return new WaitForSeconds(.1f); // to be sure not to make a condition race with the dissolver of the object

            // Fade in client
            newChild.GetComponent<NeedsAISystem>().FadeObject(false, FadeInSpeedInStartOfLevel); // Reverse dissolving the client

            // Delay between client spawning
            yield return new WaitForSeconds(timeBetweenClientsSpawnInStartOfLevel);
        }

        #endregion

        // instatiate toys by the needs in level

        // reset timer

        // reset progress bar and configure it by the maximum number of needs in level

        // Enable player controller
    }

    public Need[] GetCurrentLevelNeddsArr()
    {
        return currentLevelNeedsArr;
    } // To be called by the client

}
