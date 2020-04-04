using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpObject : MonoBehaviour
{
    [SerializeField] float floatingSpeed = 2f;
    [SerializeField] float floatingHeight = 2f;
    [SerializeField] float popZOffset = 2;
    [SerializeField] float popXOffset = 4;

    private float startingY;
    private Transform clientTransform;

    private void Start()
    {
        clientTransform = GetComponentInParent<NeedsAISystem>().transform;
        startingY = transform.position.y;
    }

    private void LateUpdate()
    {
        float newYToMove = startingY + floatingHeight * Mathf.Sin(floatingSpeed * Time.time);
        transform.position = new Vector3(CalculateXPopRelativeToPlayer(),
            newYToMove,
            CalculateZPopRelativeToPlayer());
    }


    private float CalculateZPopRelativeToPlayer()
    {
        return clientTransform.position.z + ((0 - clientTransform.position.z) / popZOffset);
    }

    private float CalculateXPopRelativeToPlayer()
    {
        return clientTransform.position.x + ((0 - clientTransform.position.x) / popXOffset);
    }
}
