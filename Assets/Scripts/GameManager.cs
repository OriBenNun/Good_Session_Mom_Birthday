using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singelton

    [SerializeField] private Transform clientsTransform = null;
    private void Awake()
    {
        if (GameManager.instance == null)
        {
            GameManager.instance = this;
        }  // Singelton
    }

    public void SetGameObjectAsClientsChildren(Transform t)
    {
        t.parent = clientsTransform;
    }

}
