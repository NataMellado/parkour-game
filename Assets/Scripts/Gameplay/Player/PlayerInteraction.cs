using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : NetworkBehaviour
{

    private Animator animator;
    private StarterAssetsInputs input;
    private bool isPunching = false;

    private int upperBodyLayerIndex;

    private Coroutine singlePunchCoroutine = null;

    [SerializeField]
    private PlayerColliderDetection playerColliderDetection;

    private void Start()
    {
        if (IsOwner)
        {
            animator = GetComponent<Animator>();
            input = GetComponent<StarterAssetsInputs>();
            upperBodyLayerIndex = animator.GetLayerIndex("UpperBody");
        }
    }


    private IEnumerator Punch()
    {
        animator.SetTrigger("punchTrigger");
        isPunching = true;
        animator.SetLayerWeight(upperBodyLayerIndex, 1f);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
        float stateInfoLength = stateInfo.length + 0.3f;

        if (singlePunchCoroutine != null)
        {
            StopCoroutine(singlePunchCoroutine);
            singlePunchCoroutine = null;
        }
        singlePunchCoroutine = StartCoroutine(DeactivatePunching(stateInfoLength));
        yield return null;
    }

    private IEnumerator DeactivatePunching(float stateInfoLength)
    {
        yield return new WaitForSeconds(stateInfoLength);
        Debug.Log("Deactivate Punching");
        animator.SetLayerWeight(upperBodyLayerIndex, 0);
        isPunching = false;
    }

    private void CalculateEffectivePunch()
    {
        if (isPunching)
        {

        }

    }

    public void OnPunchHit()
    {
        if (!IsOwner) { return; }
        Debug.Log("Golpe efectivo registrado");
        playerColliderDetection.Interact();
    }
    public void OnPunchStart()
    {

        Debug.Log("Golpe comenzado");
    }
    public void OnPunchEnd()
    {

        Debug.Log("Golpe terminado");
    }
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Punch());
        }
    }
    //private IEnumerator ResetPunching()
    //{

    //    if (!IsOwner) yield return null;
    //    // Esperar hasta que la animación de golpe termine
    //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
    //    float animationLength = stateInfo.length;

    //    yield return new WaitForSeconds(animationLength);
    //    if (cancelLastPunch)
    //    {
    //        yield return null;
    //    }
    //    else
    //    {
    //        isPunching = false;
    //        //animator.SetLayerWeight(upperBodyLayerIndex, 0f);
    //    }
    //}



    //private IEnumerator Punch()
    //{
    //    Debug.Log("Punching");
    //    isPunching = true;
    //    animator.SetTrigger("punchTrigger");
    //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
    //    float stateInfoLength = stateInfo.length;

    //    if (singlePunchCoroutine != null)
    //    {
    //        StopCoroutine(singlePunchCoroutine);
    //        singlePunchCoroutine = null;
    //        singlePunchCoroutine = StartCoroutine(DeactivatePunching(stateInfoLength));
    //    }
    //    else
    //    {
    //        singlePunchCoroutine = StartCoroutine(DeactivatePunching(stateInfoLength));
    //    }

    //    yield return null;
    //}

    //private void DoublePunch()
    //{
    //    animator.SetLayerWeight(upperBodyLayerIndex, 1f);
    //    animator.SetTrigger("punchTrigger");
    //    animator.SetFloat("Punch", 2f);
    //    if (doublePunchCoroutine != null)
    //    {
    //        StopCoroutine(doublePunchCoroutine);
    //        doublePunchCoroutine = null;
    //    }
    //    doublePunchCoroutine = StartCoroutine(DeactivatePunches());
    //}

    //private IEnumerator DeactivatePunches()
    //{
    //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
    //    float stateInfoLength = stateInfo.length + 0.5f;
    //    Debug.Log("Esperó segunda animación, desactivando");
    //    animator.SetLayerWeight(upperBodyLayerIndex, 0f);
    //    isPunching = false;
    //    isDoublePunching = false;
    //    animator.SetFloat("Punch", 0f);
    //    yield return null;
    //}

    //private IEnumerator DeactivatePunching(float stateInfoLength)
    //{
    //    yield return new WaitForSeconds(stateInfoLength);
    //    Debug.Log("Ya no está punching");
    //    animator.SetLayerWeight(upperBodyLayerIndex, 0f);
    //    isPunching = false;
    //}

}
