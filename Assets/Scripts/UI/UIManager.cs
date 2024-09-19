using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Tbvl.GameManager;
using UnityEngine.Rendering;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.InputSystem;
using StarterAssets;

public class UIManager : MonoBehaviour
{
    
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private Button disconnectButton;

    [SerializeField]
    private Canvas mainMenuCanvas;

    [SerializeField]
    private Canvas consoleCanvas;

    private GameManager gameManager;

    public bool pauseMenu = true;
    public KeyCode toggleKey = KeyCode.Escape;

    public bool isConsole = false;
    public KeyCode toggleConsoleKey = KeyCode.F9;

    public Camera gameCamera;
    public Camera menuCamera;

    private PlayerInput playerInput;

    private bool isConnected = false;



    private void Awake() {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        connectButton.onClick.AddListener(OnConnectClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);
        gameManager = FindObjectOfType<GameManager>();
        consoleCanvas.gameObject.SetActive(false);

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
    private void HandleConnectionStateChanged(bool Connected)
    {
        if (Connected)
        {
            isConnected = true;
            if (isConnected)
            {
                var jugadorLocal = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                playerInput = jugadorLocal.GetComponent<PlayerInput>();
            }
            // Jugador conectado
            togglePauseMenu();
            // Buscar playerInput en el jugador local
        }
        else
        {
            // Jugador desconectado
            isConnected = false;
            pauseMenu = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SwitchToMenuCamera();
            gameCamera.gameObject.SetActive(true);
            gameCamera.GetComponent<AudioListener>().enabled = false;
        }
    }

    

    private void OnHostClicked()
    {
        // Si está jugando, que no haga nada
        if (isConnected)
            return;
        Debug.Log("Host clicked");
        gameManager.StartHost();
    }

    private void OnClientClicked()
    {
        // Si está jugando, que no haga nada
        if (isConnected)
            return;
        Debug.Log("Client clicked");
        gameManager.StartClient();
    }

    private void OnConnectClicked()
    {
        // Si está jugando, que no haga nada
        if (isConnected)
            return;

        Debug.Log("Connect clicked");

        // Check if server ip input field is empty
        if (gameManager.serverIpText.text.Trim() == "")
        {
            Debug.LogError("Server IP is empty");
            return;
        }

        gameManager.StartClient();
    }
    private void OnDisconnectClicked()
    {
        // Si no está jugando, que no haga nada
        if (!isConnected)
            return;

        Debug.Log("Disconnect clicked");

        gameManager.Disconnect();
    }
    private void HideUICanvas(){
        mainMenuCanvas.gameObject.SetActive(false);
    }

    private void ShowUICanvas()
    {
        mainMenuCanvas.gameObject.SetActive(true);
    }
    private void SwitchToGameCamera()
    {
        Debug.Log("Switching to game camera");
        // Cambiar cámaras
        gameCamera.gameObject.SetActive(true);
        gameCamera.GetComponent<AudioListener>().enabled = true;

        menuCamera.GetComponent<AudioListener>().enabled = false;
        menuCamera.gameObject.SetActive(false);
        HideUICanvas();
    }

    private void SwitchToMenuCamera()
    { 
        Debug.Log("Switching to menu camera");
        menuCamera.gameObject.SetActive(true);
        // Habilitar audio listener de la cámara
        menuCamera.GetComponent<AudioListener>().enabled = true;

        gameCamera.GetComponent<AudioListener>().enabled = false;
        gameCamera.gameObject.SetActive(false);
        ShowUICanvas();
    }
    private void togglePauseMenu()
    {
        pauseMenu = !pauseMenu;

        if (pauseMenu)
        {
            SwitchToMenuCamera();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerInput.enabled = false;
        }
        else
        {
            SwitchToGameCamera();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerInput.enabled = true;
        }
    }
    private void toggleConsole()
    {
        isConsole = !isConsole;

        if (isConsole)
        {
            consoleCanvas.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (isConnected)
            playerInput.enabled = false;
        }
        else
        {
            consoleCanvas.gameObject.SetActive(false);

            if (isConnected)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                playerInput.enabled = true;
            }
        }
    }

    private void Update() {


        // Si presiona la tecla A, el cursor se muestra
        if (Input.GetKeyDown(toggleKey) && isConnected)
        {
            Debug.Log("Toggle pause menu");
            togglePauseMenu();
        }

        if (Input.GetKeyDown(toggleConsoleKey))
        {

            Debug.Log("Toggle console");
            // Mostrar/ocultar la consola
            toggleConsole();
        }
    }
    
}

