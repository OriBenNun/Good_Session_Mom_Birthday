using System;
using UnityEngine;

public class DissolveMaterialCreatorController : MonoBehaviour
{
    private Texture2D[] m_originalTextures;
    private Texture2D[] m_originalNormals;

    //private Material originalMat;
    //private Material dissolveMat;

    private Material[] originalMats;
    private Material[] dissolveMats;

    private bool isDissolving;
    private bool isReverseDissolving;
    private float dissolveSpeed;

    private float lValue = 0;
    private bool isVisible = true;

    public event Action OnFinishedDissolve;

    [SerializeField] private bool isStartingTransparent = true;

    // create the dissolve material,
    // stores the original material and changing to the dissolve material
    private void Awake()
    {
        originalMats = GetComponent<Renderer>().materials;

        m_originalNormals = new Texture2D[originalMats.Length];
        m_originalTextures = new Texture2D[originalMats.Length];

        dissolveMats = new Material[originalMats.Length];

        for (int i = 0; i < originalMats.Length; i++)
        {

            string originalName = originalMats[i].name;
            if (originalMats[i].GetTexture("_BumpMap"))
            {
                m_originalNormals[i] = (Texture2D)originalMats[i].GetTexture("_BumpMap");
            }

            m_originalTextures[i] = (Texture2D)originalMats[i].mainTexture;

            dissolveMats[i] = new Material(Shader.Find("Shader Graphs/Dissolve"))
            {
                name = originalName + "_DissolveMat"
            };

            if (m_originalTextures[i] != null)
            {
                dissolveMats[i].SetTexture("originalTexture", m_originalTextures[i]);
            }

            if (m_originalNormals[i] != null)
            {
                dissolveMats[i].SetTexture("originalNormal", m_originalNormals[i]);
            }

            if (dissolveMats.Length == 0)
            {
                Debug.LogError(name + " Got no dissolve materials, and you will divide by 0 if you will try to dissolve it. stopping now");
                Debug.Break();
            }
        }

        GetComponent<Renderer>().materials = dissolveMats;

        if (isStartingTransparent)
        {
            for (int i = 0; i < dissolveMats.Length; i++)
            {
                dissolveMats[i].SetFloat("alphaClipThreshold", 1);
                isVisible = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (isDissolving)
        {
            lValue += (dissolveSpeed * Time.deltaTime);
            for (int i = 0; i < dissolveMats.Length; i++)
            {
                dissolveMats[i].SetFloat("alphaClipThreshold", Mathf.Lerp(0, 1, lValue));
            }

            if (lValue >= 1)
            {
                isDissolving = false;
                lValue = 0;
                isVisible = false;
                OnFinishedDissolve?.Invoke();
            }

        }
        else if (isReverseDissolving)
        {
            lValue += (dissolveSpeed * Time.deltaTime);
            for (int i = 0; i < dissolveMats.Length; i++)
            {
                dissolveMats[i].SetFloat("alphaClipThreshold", Mathf.Lerp(1, 0, lValue));
            }

            if (lValue >= 1)
            {
                isReverseDissolving = false;
                lValue = 0;
                isVisible = true;
            }
        }
    }

    /// <summary>
    /// Starts dissolving the object. speed is set to 1 if not specified different
    /// </summary>
    /// <param name="speed"></param>
    public void StartDissolve(float speed = 1f)
    {
        if (isVisible)
        {
            isDissolving = true;
            dissolveSpeed = speed;
        }

        else
        {
            Debug.LogError(name + " is already invisible!");
        }
    }

    /// <summary>
    /// Starts reverse the dissolved object. speed is set to 1 if not specified different
    /// </summary>
    /// <param name="speed"></param>
    public void StartReverseDissolve(float speed = 1f)
    {
        if (!isVisible)
        {
            isReverseDissolving = true;
            dissolveSpeed = speed;
        }

        else
        {
            Debug.LogError(name + " is already visible@@");
        }
    }

    public bool GetIsVisible() => isVisible;
}
