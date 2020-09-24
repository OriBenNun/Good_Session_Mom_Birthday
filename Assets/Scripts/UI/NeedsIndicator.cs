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

    public void CreateNeedIndicator(PopUpObject popUpObject)
    {
        currentNeedObject = Instantiate(popUpObject.gameObject, popUpInstatiatePos.position, Quaternion.identity, this.transform);

        currentCanvas = Instantiate(popupBackgroung, currentNeedObject.transform);

        currentNeedObject.transform.Rotate(Vector3.right, 80, Space.Self);
    }

    public void UpdatePopAnimator(float urgentBlend)
    {
        currentCanvas.GetComponentInChildren<Animator>().SetFloat("urgentBlend", urgentBlend);
    }

    public void TriggerExplodeAnimation()
    {
        currentCanvas.GetComponentInChildren<Animator>().SetTrigger("explodeTrigger");
    }

    public void TriggerSucceedAnimation()
    {
        currentCanvas.GetComponentInChildren<Animator>().SetTrigger("winTrigger");
    }

    public void DestroyNeedIndication()
    {
        Destroy(currentNeedObject);
        currentNeedObject = null;
    }

    public void HideIndicator(bool isHidden)
    {
        if (currentNeedObject != null)
        {
            currentNeedObject.GetComponentInChildren<Renderer>().enabled = !isHidden;
            currentCanvas.enabled = !isHidden;
        }
    }
}
