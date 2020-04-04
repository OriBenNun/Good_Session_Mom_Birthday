using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorPicker : MonoBehaviour
{
    [SerializeField] float minTimeBetweenAnim = 3f;
    [SerializeField] float maxTimeBetweenAnim = 7f;
    private Animator animator;
    private float timer = 0;

    private float currentTimerMax;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        currentTimerMax = Random.Range(minTimeBetweenAnim, maxTimeBetweenAnim);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < currentTimerMax)
        {
            timer += Time.deltaTime;
        }
        
        else
        {
            timer = 0;
            currentTimerMax = Random.Range(minTimeBetweenAnim, maxTimeBetweenAnim);

            var animations = animator.GetCurrentAnimatorClipInfoCount(0);
            Debug.Log(animations);
        }
    }
}
