using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Tbvl.GameManager;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{
    
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    [SerializeField]
    private Canvas mainMenuCanvas;
    
    private GameManager gameManager;

    public bool pauseMenu = true;
    public KeyCode toggleKey = KeyCode.Escape;

    public Camera gameCamera;
    public Camera menuCamera;


    private void Awake() {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        gameManager = FindObjectOfType<GameManager>();

        // Suscribir a cambios en la conexión
        gameManager.OnConnectionStateChanged += HandleConnectionStateChanged;
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento para evitar memory leaks
        if (gameManager != null)
        {
            gameManager.OnConnectionStateChanged -= HandleConnectionStateChanged;
        }
    }

    // Callback para cuando cambia el estado de la conexión
    private void HandleConnectionStateChanged(bool isConnected)
    {
        if (isConnected)
        {
            // Jugador conectado
            SwitchToGameCamera();
        }else
        {
            // Jugador desconectado
            SwitchToMenuCamera();
        }
    }

    private void OnHostClicked(){
        Debug.Log("Host clicked");
        gameManager.StartHost();
        HideUICanvas();
    }

    private void OnClientClicked(){
        Debug.Log("Client clicked");
        gameManager.StartClient();
        HideUICanvas();
    }
    private void HideUICanvas(){
        mainMenuCanvas.gameObject.SetActive(false);
    }
    private void SwitchToGameCamera()
    {
        Debug.Log("Switching to game camera");
        gameCamera.gameObject.SetActive(true);
        menuCamera.gameObject.SetActive(false);
    }

    private void SwitchToMenuCamera()
    { 
        Debug.Log("Switching to menu camera");
        menuCamera.gameObject.SetActive(true);
        gameCamera.gameObject.SetActive(false);
    }
    private void togglePauseMenu()
    {
        pauseMenu = !pauseMenu;

        if (pauseMenu)
        {
            SwitchToMenuCamera();
            Cursor.visible = true;
        }
        else
        {
            SwitchToGameCamera();
        }
    }

    private void Update() {


        // Si presiona la tecla A, el cursor se muestra
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("Toggle pause menu");
            togglePauseMenu();
        }
    }
    
}

