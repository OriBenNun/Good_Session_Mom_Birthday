using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorPicker : MonoBehaviour
{
    [SerializeField] float minTimeBetweenAnim = 3f;
    [SerializeField] float maxTimeBetweenAnim = 7f;

    [SerializeField] string[] triggersNames;
    private Animator animator;
    private float timer = 0;

    private float currentTimerMax;
    void Start()
    {
        animator = GetComponent<Animator>();
        currentTimerMax = Random.Range(minTimeBetweenAnim, maxTimeBetweenAnim);
    }

    void Update()
    {
        if (timer < currentTimerMax)
        {
            timer += Time.deltaTime;
        }
        
        else
        {
            Debug.Log("here1");
            timer = 0;
            currentTimerMax = Random.Range(minTimeBetweenAnim, maxTimeBetweenAnim);

            int rand = Random.Range(0, triggersNames.Length);
            animator.SetTrigger(triggersNames[rand]);
        }
    }
}
