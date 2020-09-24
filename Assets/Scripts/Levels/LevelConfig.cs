using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Levels Configuration")]
public class LevelConfig : ScriptableObject
{
    [SerializeField] public int levelNumber = 0;
    [SerializeField] public int numberOfClients = 0;
    [SerializeField] public int maximumNeedsToComplete = 0;
    [SerializeField] public Need[] needsTypeInLevel = null;
}
