using UnityEngine;

public class NameTagFollowCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Busca la c�mara principal del jugador local
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        // Hacer que el texto mire hacia la c�mara
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}