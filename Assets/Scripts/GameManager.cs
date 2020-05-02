using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.Soundy;
using Doozy.Engine.Progress;
using Doozy.Engine.UI;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singelton

    [Header("Level Configs")]
    [SerializeField] int firstLevelNumber = 1;
    [SerializeField] LevelConfig[] levelsConfigs = null;
    [SerializeField] GameObject[] availableClients = null;

    [Header("Hirarchy Organise")]
    [SerializeField] Transform clientsParentTransform = null;
    [SerializeField] Transform toysParentTransform = null;

    [Header("General Gameplay Settings")]
    [SerializeField] float levelTimeLengthSeconds = 90; // will not be updated during runtime if changed in inspector
    [SerializeField] float failedProgressDecrease = .7f;
    [SerializeField] Transform[] clientsSpawnPoints;
    [SerializeField] Transform[] toysSpawnPoints;

    [Header("Design Settings")]
    [SerializeField] float timeBetweenSpawnInStartOfLevel = 1f;
    [SerializeField] float FadeInSpeedInStartOfLevel = .5f;

    [Header("Sounds")]
    [SerializeField] SoundyData spawnClientSound;
    [SerializeField] SoundyData spawnToySound;
    [SerializeField] SoundyData starTurnedOnSound;
    [SerializeField] SoundyData clickFailSound;
    [SerializeField] SoundyData needFulfilledSound;
    [SerializeField] SoundyData needFailedSound;
    [SerializeField] SoundyData newNeedSound;

    [Header("HUD")]
    [SerializeField] Progressor timerProgressor;
    [SerializeField] Progressor needsFulfilledProgressor;
    [SerializeField] Image firstStar;
    [SerializeField] Image firstStarLine;
    [SerializeField] Image secondStar;
    [SerializeField] Image secondStarLine;
    [SerializeField] Image thirdStar;
    [SerializeField] Image thirdStarLine;
    [SerializeField] Image filledStarSettings;
    [SerializeField] Image emptyStarSettings;

    public event Action onLevelFinished;
    public event Action onLevelLoaded;


    private int currentLevelLoadedNumber = 0;
    private GameObject[] currentClientsInScene;
    private List<GameObject> currentToysInScene;
    private Need[] currentLevelNeedsArr; // for the client to ask once it spawned
    private UIPopup finishedLevelPopup;

    private float currentLevelTimer;
    private float currentLevelFulfilledNeeds;

    private int numberOfStarsTurnedOn;

    public bool isGameInPlayState = false;


    private const float destroyDelay = 2f;

    private const string finishedPopup1Star = "FinishedLevelPopup_1Star";
    private const string finishedPopup2Stars = "FinishedLevelPopup_2Stars";
    private const string finishedPopup3Stars = "FinishedLevelPopup_3Stars";
    private const string finishedPopupNoStars = "FinishedLevelPopup_NoStars";

    private void Awake()
    {
        if (GameManager.instance == null)
        {
            GameManager.instance = this;
        }  // Singelton

        currentLevelTimer = levelTimeLengthSeconds;
    }

    private void Start()
    {
        timerProgressor.SetMax(levelTimeLengthSeconds);
        timerProgressor.SetMin(0);

        if (currentLevelLoadedNumber == 0)
        {
            StartCoroutine("LoadLevelFromConfig", firstLevelNumber);
        }
    }

    private void Update()
    {
        if (isGameInPlayState)
        {
            currentLevelTimer -= Time.deltaTime;

            timerProgressor.SetValue(currentLevelTimer); // updated the HUD timer Doozy progressor value

            if (currentLevelTimer <= 0)
            {
                LevelFinished();
            }
        }
    }
    private IEnumerator DestroyCurrentLevel()
    {
        ToggleGameHUDUpdateAndPlayerController(false);

        // Destroy existing clients
        for (int i = 0; i < currentClientsInScene.Length; i++)
        {
            // Fade out client
            currentClientsInScene[i].GetComponent<NeedsAISystem>().FadeObject(true, FadeInSpeedInStartOfLevel); // Reverse dissolving the client

            // Hide need indicator
            currentClientsInScene[i].GetComponent<NeedsIndicator>().HideIndicator(true);
            

            // Delay between client spawning
            yield return new WaitForSeconds(timeBetweenSpawnInStartOfLevel / 2);

            // Play sfx
            //SoundyManager.Play(spawnClientSound);

            // Destroy Client Gameobject
            Destroy(currentClientsInScene[i], destroyDelay);
        }

        // Destroy existing toys
        for (int i = 0; i < currentToysInScene.Count; i++)
        {
            // Fade out client
            currentToysInScene[i].GetComponent<PickableToy>().FadeObject(true, FadeInSpeedInStartOfLevel); // Reverse dissolving the client

            // Delay between client spawning
            yield return new WaitForSeconds(timeBetweenSpawnInStartOfLevel / 2);

            // Play sfx
            //SoundyManager.Play(spawnToySound);

            // Destroy Client Gameobject
            Destroy(currentToysInScene[i], destroyDelay);
        }

        yield return new WaitForSeconds(5);
    }

    /// <summary>
    /// Instatiating Clients, Toys. as well as reseting the Timer and progress bar for the current level
    /// </summary>
    /// <param name="levelNumberToLoad"></param>
    /// <returns></returns>
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
            currentLevelLoadedNumber = 1;
            Debug.LogError("Something is wrong with level loader on level number " + levelNumberToLoad + ". Probably u ran out of levels loading first level for now");
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
            int randomClientIndex = UnityEngine.Random.Range(0, availableClientsIndexes.Count);

            availableClientsIndexes.RemoveAt(randomClientIndex);

            clientsToLoad[i] = availableClients[randomClientIndex];
        }

        // Instatiating the clients array that was formed in the randomness handler
        for (int i = 0; i < clientsToLoad.Length; i++)
        {
            var newChild = Instantiate(clientsToLoad[i], clientsSpawnPoints[i].position, Quaternion.identity, clientsParentTransform);
            currentClientsInScene[i] = newChild;

            yield return new WaitForSeconds(.01f); // to be sure not to make a condition race with the dissolver of the object

            // Fade in client
            newChild.GetComponent<NeedsAISystem>().FadeObject(false, FadeInSpeedInStartOfLevel); // Reverse dissolving the client

            // Delay between client spawning
            yield return new WaitForSeconds(timeBetweenSpawnInStartOfLevel);

            // Play sfx
            SoundyManager.Play(spawnClientSound);
        }

        #endregion

        #region Instatiate toys by the needs in level

        // reset the currentToysInScene for later use (to destroy at end of level)
        currentToysInScene = new List<GameObject>();
        int toySpawnPointIndex = 0;
        for (int i = 0; i < currentLevelNeedsArr.Length; i++)
        {
            var need = currentLevelNeedsArr[i];
            if (need.needObject)
            {
                var newToy = Instantiate(need.needPrefab, toysSpawnPoints[toySpawnPointIndex++].position, Quaternion.identity, toysParentTransform);
                currentToysInScene.Add(newToy);

                yield return new WaitForSeconds(.01f); // to be sure not to make a condition race with the dissolver of the object

                // Fade in toy
                newToy.GetComponent<PickableToy>().FadeObject(false, FadeInSpeedInStartOfLevel); // Reverse dissolving the toy

                // Delay between toy spawning
                yield return new WaitForSeconds(timeBetweenSpawnInStartOfLevel);

                // play sfx
                SoundyManager.Play(spawnToySound);
            }
        }
        #endregion

        ResetHUD(levelToLoad);

        LevelNumberAnnouncerPopup(); // The title popup that shows the number of the level

        PlayerManager.instance.FadeObject(false); // To handle a niche bug where the level ends exactly when the player is fade out

        onLevelLoaded?.Invoke();
        // Enable player controller
        ToggleGameHUDUpdateAndPlayerController(true);
    }

    private void ResetHUD(LevelConfig levelToLoad)
    {
        // reset timer
        timerProgressor.ResetValueTo(ResetValue.ToMaxValue);
        currentLevelTimer = levelTimeLengthSeconds;

        // reset progress bar and configure it by the maximum number of needs in level
        currentLevelFulfilledNeeds = 0;
        needsFulfilledProgressor.SetMax(levelToLoad.maximumNeedsToComplete);
        needsFulfilledProgressor.SetMin(0);
        needsFulfilledProgressor.ResetValueTo(ResetValue.ToMinValue);

        // reset the stars color
        numberOfStarsTurnedOn = 0;
        firstStar.color = emptyStarSettings.color;
        firstStarLine.color = emptyStarSettings.color;
        secondStar.color = emptyStarSettings.color;
        secondStarLine.color = emptyStarSettings.color;
        thirdStar.color = emptyStarSettings.color;
        thirdStarLine.color = emptyStarSettings.color;
    }

    private void ToggleGameHUDUpdateAndPlayerController(bool isPlayerCanControl)
    {
        isGameInPlayState = isPlayerCanControl;
        PlayerManager.instance.isPlayerAbleToControl = isPlayerCanControl;
    }

    /// <summary>
    /// To be called by the player when a need is fulfilled / failed (took too long)
    /// </summary>
    /// <param name="isSucceeded"></param>
    public void UpdateFulfilledNeedsProgress(bool isSucceeded)
    {
        if (isSucceeded)
        {
            SoundyManager.Play(needFulfilledSound);
            currentLevelFulfilledNeeds++;
        }
        else
        {
            SoundyManager.Play(needFailedSound);
            if (currentLevelFulfilledNeeds > 0)
            {
                currentLevelFulfilledNeeds-= failedProgressDecrease;
            }
        }
        needsFulfilledProgressor.SetValue(currentLevelFulfilledNeeds);
    }

    public void OnProgressorProgressChanged()
    {
        if (!isGameInPlayState) { return; }

        // First Star

        if (needsFulfilledProgressor.Progress >= 0.33f && numberOfStarsTurnedOn == 0) 
        {
            numberOfStarsTurnedOn++;
            firstStar.GetComponent<Animator>().SetTrigger("Pop");
            SoundyManager.Play(starTurnedOnSound);
            firstStar.color = filledStarSettings.color;
            firstStarLine.color = filledStarSettings.color;
        }
        else if (needsFulfilledProgressor.Progress < 0.3f && numberOfStarsTurnedOn == 1)
        {
            numberOfStarsTurnedOn--;
            //SoundyManager.Play(starTurnedOnSound);
            firstStar.color = emptyStarSettings.color;
            firstStarLine.color = emptyStarSettings.color;
        }

        //Second Star

        else if (needsFulfilledProgressor.Progress >= 0.66f && numberOfStarsTurnedOn == 1)
        {
            numberOfStarsTurnedOn++;
            secondStar.GetComponent<Animator>().SetTrigger("Pop");
            SoundyManager.Play(starTurnedOnSound);
            secondStar.color = filledStarSettings.color;
            secondStarLine.color = filledStarSettings.color;
        }
        else if (needsFulfilledProgressor.Progress < 0.63f && numberOfStarsTurnedOn == 2)
        {
            numberOfStarsTurnedOn--;
            //SoundyManager.Play(starTurnedOnSound);
            secondStar.color = emptyStarSettings.color;
            secondStarLine.color = emptyStarSettings.color;
        }

        else if (needsFulfilledProgressor.Value == needsFulfilledProgressor.MaxValue)
        {
            numberOfStarsTurnedOn++;
            thirdStar.GetComponent<Animator>().SetTrigger("Pop");
            //SoundyManager.Play(starTurnedOnSound);
            thirdStar.color = filledStarSettings.color;
            thirdStarLine.color = filledStarSettings.color;

            LevelFinished();
        }
    }

    private void LevelFinished()
    {
        // To fix a bug where you just fulfilled a need and the timer is over before the progress reached the third star, so manually changing it
        needsFulfilledProgressor.SetValue(currentLevelFulfilledNeeds, true);
        if (currentLevelFulfilledNeeds >= needsFulfilledProgressor.MaxValue)
        {
            numberOfStarsTurnedOn = 3;
        }

        ToggleGameHUDUpdateAndPlayerController(false);

        onLevelFinished?.Invoke();

        StartCoroutine("DestroyCurrentLevel");

        string messageText = "בשלב " + currentLevelLoadedNumber + " קיבלת:";

        switch (numberOfStarsTurnedOn)
        {
            case 1:
                finishedLevelPopup = UIPopup.GetPopup(finishedPopup1Star);
                break;
            case 2:
                finishedLevelPopup = UIPopup.GetPopup(finishedPopup2Stars);
                break;
            case 3:
                finishedLevelPopup = UIPopup.GetPopup(finishedPopup3Stars);
                break;

            default:
                finishedLevelPopup = UIPopup.GetPopup(finishedPopupNoStars);
                messageText = "בשלב " + currentLevelLoadedNumber + " לא קיבלת כוכבים, אבל אפשר לנסות שוב";
                break;
        }

        finishedLevelPopup.Data.SetLabelsTexts(messageText);
        finishedLevelPopup.Data.SetButtonsCallbacks(OnTryAgainClicked, OnNextLevelClicked);

        finishedLevelPopup.Show();
    }

    private void OnTryAgainClicked()
    {
        StartCoroutine("LoadLevelFromConfig", currentLevelLoadedNumber);
        finishedLevelPopup.Hide();
    }

    private void OnNextLevelClicked()
    {
        StartCoroutine("LoadLevelFromConfig", ++currentLevelLoadedNumber);
        finishedLevelPopup.Hide();
    }

    private void LevelNumberAnnouncerPopup()
    {
        UIPopup popup = UIPopup.GetPopup("LevelAnnouncerPopup");
        string text = "שלב " + currentLevelLoadedNumber;
        popup.Data.SetLabelsTexts(text);
        popup.Show();
    }

    /// <summary>
    /// To be called by the client
    /// </summary>
    /// <returns></returns>
    public Need[] GetCurrentLevelNeddsArr()
    {
        return currentLevelNeedsArr;
    }

    public void SetGameObjectAsClientsChildren(Transform t)
    {
        t.parent = clientsParentTransform;
    }

    public void PlayClickFailSound()
    {
        SoundyManager.Play(clickFailSound);
    }
    public void PlayNewNeedSound()
    {
        SoundyManager.Play(newNeedSound);
    }
}
