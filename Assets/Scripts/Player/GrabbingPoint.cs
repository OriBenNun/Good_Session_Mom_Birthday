using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingPoint : MonoBehaviour
{
    [SerializeField] Transform handR;
    [SerializeField] Transform handL; // << the gameobject is a child of this hand

    [SerializeField] float moveSmoothSpeedMultiplier = 10;

    Vector3 startingPos;

    private void Start()
    {
        startingPos = transform.position;
    }
    private void Update()
    {
        Vector3 newPos = handL.position + (handR.position - handL.position) / 2;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * moveSmoothSpeedMultiplier);
    }
}
