using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInstatiateFromEvent : MonoBehaviour
{
    [SerializeField] ParticleSystem particle = null;
    [SerializeField] float timeBeforeDestroy = 5;
    public void EmitParticles()
    {
        Debug.Log("Emitiing particles");
        ParticleSystem particleSystem = Instantiate(particle, transform.position, Quaternion.identity);
        Destroy(particleSystem, timeBeforeDestroy);
    }
}
