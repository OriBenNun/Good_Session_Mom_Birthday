using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpObject : MonoBehaviour
{
    [SerializeField] float floatingSpeed = 2f;
    [SerializeField] float floatingHeight = 2f;

    private float startingY;
    private void Start()
    {
        startingY = transform.position.y;
    }

    private void Update()
    {
        float newYToMove = startingY + floatingHeight * Mathf.Sin(floatingSpeed * Time.time);
        transform.position = new Vector3(transform.position.x, newYToMove, transform.position.z);
    }
}
