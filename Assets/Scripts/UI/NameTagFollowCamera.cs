using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTagFollowCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Busca la cámara principal del jugador local
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No se encontró la cámara principal");
                return;
            }
        }

        // Hacer que el texto mire hacia la cámara
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}