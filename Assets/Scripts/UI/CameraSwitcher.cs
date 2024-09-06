using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera gameCamera;
    public Camera menuCamera;

    public void SwitchToGameCamera()
    {
        menuCamera.gameObject.SetActive(false);
        gameCamera.gameObject.SetActive(true);
    }

    public void SwitchToMenuCamera()
    {
        gameCamera.gameObject.SetActive(false);
        menuCamera.gameObject.SetActive(true);
    }

}
