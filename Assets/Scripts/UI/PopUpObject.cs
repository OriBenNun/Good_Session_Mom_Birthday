using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpObject : MonoBehaviour
{
    [SerializeField] float floatingSpeed = 1.5f;
    [SerializeField] float floatingHeight = 0.4f;
    [SerializeField] float popZOffset = 15;
    [SerializeField] float popXOffset = 20;

    //[SerializeField] float rotationSpeed = 2;

    private float startingY;
    private Transform clientTransform;

    private void Start()
    {
        clientTransform = GetComponentInParent<NeedsAISystem>().transform;
        startingY = transform.position.y;
    }

    private void Update()
    {
        // transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed); // causing the background to rotate as well, TODO can fix with another game object above the mesh and the canvas
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
