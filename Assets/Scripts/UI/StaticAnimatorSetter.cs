using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAnimatorSetter : MonoBehaviour
{
    [SerializeField]
    public int animNumber = 1;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        //animator.SetInteger("Anim", animNumber);


    }

}
