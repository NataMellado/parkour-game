using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] float CrossFadeTime = 0.08f;
    public bool inAction;
    EnvironmentScanner environmentScanner;
    Animator animator;
    ThirdPersonController playerController;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<ThirdPersonController>();    
    }

    private void Update()
    {
        if (Input.GetButton("Jump") && !inAction)
        {
            var hitData = environmentScanner.ObstacleCheck();
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        Debug.Log("Parkour action: " + action.AnimName);
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;

        playerController.enabled = false;
        animator.applyRootMotion = true;
        playerController.Grounded = false;
        animator.SetBool("Grounded", false);
        animator.CrossFade(action.AnimName, CrossFadeTime);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
            Debug.LogError("Animation not found: " + action.AnimName);

        // Debug.Log("Obstacle found: " + hitData.forwardHit.transform.name);
        // yield return new WaitForSeconds(animState.length);

        
        float timer = 0f;
        // Bucle para esperar a que termine la animación y salir si se está en transición
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            // Si se activa el giro hacia el obstáculo se rota el player
            if (action.RotateToObstacle)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotationSmoothTime*10*400 * Time.deltaTime);

            // Si se activa el match target se activa la función para que el player se mueva hacia el obstáculo
            if (action.EnableTargetMatching)
                MatchTarget(action);

            // Si se está en transición y ha pasado un tiempo se sale del bucle
            if (animator.IsInTransition(0) && timer > 0.8f)
                break;

            // Salir del bucle si se ha terminado la animación
            yield return null;
        }

        // Esperar a que termine la animación 
        yield return new WaitForSeconds(action.PostActionDelay);
        playerController.SetVerticalVelocity(0);
        playerController.SetJumping(false);
        // Restaurar la rotación del player a la original 
        animator.applyRootMotion = false;
        playerController.enabled = true;
        playerController.Grounded = true;
        animator.SetBool("Grounded", true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, 
            new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }
}
