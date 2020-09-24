using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInstatiateFromEvent : MonoBehaviour
{
    [SerializeField] ParticleSystem explodeParticles = null;
    [SerializeField] ParticleSystem winParticle = null;
    public void EmitExplodeParticles()
    {
        Instantiate(explodeParticles, transform.position, Quaternion.identity);
    }

    public void EmitWinParticles()
    {
        Instantiate(winParticle, transform.position, Quaternion.identity);
    }
}
