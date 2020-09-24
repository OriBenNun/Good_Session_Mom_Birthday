using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveItemListener : MonoBehaviour
{
    public void GiveItemEvent()
    {
        PlayerManager.instance.FinishedGiveAnimation();
    }
}
