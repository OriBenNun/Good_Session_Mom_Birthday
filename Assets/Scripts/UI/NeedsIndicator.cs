using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeedsIndicator : MonoBehaviour
{
    [SerializeField] Canvas popupBackgroung = null;
    [SerializeField] Transform popUpInstatiatePos = null;

    private GameObject currentNeedObject;
    private Canvas currentCanvas;
    private Image currentImage;

    public void CreateNeedIndicator(PopUpObject popUpObject)
    {
        currentNeedObject = Instantiate(popUpObject.gameObject, popUpInstatiatePos.position, Quaternion.identity, this.transform);
        //currentNeedObject.transform.localScale = scaleModifier;

        currentCanvas = Instantiate(popupBackgroung, currentNeedObject.transform);

        currentNeedObject.transform.Rotate(Vector3.right, 80, Space.Self);
    }

    public void DestroyNeedIndication()
    {
        // TODO vfx for that
        Destroy(currentNeedObject);
        currentNeedObject = null;
    }
}
