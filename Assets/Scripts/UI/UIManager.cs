using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Tbvl.GameManager;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
    private Canvas messageTextBoxCanvas;

    [SerializeField]
    private TextMeshProUGUI messageTextBox;

    [SerializeField]
    private TextMeshProUGUI healthTextBox;

    public TMP_InputField playerNameInputField;

    private GameManager gameManager;

    public bool pauseMenu = true;
    public KeyCode toggleKey = KeyCode.Escape;

    public Camera gameCamera;
    public Camera menuCamera;

    private PlayerInput playerInput;

    private bool isConnected = false;
    private bool isConnecting;

    private bool gameFocused = true;

    private NetworkObject jugadorLocal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        connectButton.onClick.AddListener(OnConnectClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);
        gameManager = FindObjectOfType<GameManager>();
        isConnecting = gameManager.isConnecting;

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
        // TODO: Arreglar lógica de handling
        if (Connected)
        {
            //Debug.LogWarning("Jugador conectado");
            isConnected = true;
            if (isConnected)
            {
                // Logica para asignar playerInput de jugador
                // OJO: SÓLO FUNCIONA CUANDO EL JUGADOR LOCAL ES SPAWNEADO POR EL NETWORKMANAGER
                // COMO PREFAB DEFINIDO, NO FUNCIONA CUANDO EL SPAWNEO ES DE MANERA DINÁMICA
                //jugadorLocal = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                //if (jugadorLocal != null)
                //    playerInput = jugadorLocal.GetComponent<PlayerInput>();
                //else
                //    Debug.LogError("No se encontró el jugador local");
            }
            // Jugador conectado
            togglePauseMenu();
            // Buscar playerInput en el jugador local
        }
        else
        {
            //Debug.LogWarning("Jugador desconectado");
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

    private void Start()
    {

        StartCoroutine(WaitForLocalPlayer());
    }

    private IEnumerator WaitForLocalPlayer()
    {
        while (NetworkManager.Singleton == null || NetworkManager.Singleton.IsClient)
        {
            yield return null;
        }
        while (NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        // El objeto del jugador local está disponible
        jugadorLocal = NetworkManager.Singleton.LocalClient.PlayerObject;
        // Obtener el Player Input o cualquier componente
        playerInput = jugadorLocal.GetComponent<PlayerInput>();
        // Hacer lo que necesites con playerInput
    }

    private void OnHostClicked()
    {
        // Si está jugando, que no haga nada
        if (isConnected)
            return;
        //Debug.Log("Host clicked");
        gameManager.StartHost();
    }

    private void OnClientClicked()
    {
        // Si está jugando, que no haga nada
        if (isConnected)
            return;
        //Debug.Log("Client clicked");
        gameManager.StartClient();
    }

    private void OnConnectClicked()
    {
        if (isConnecting)
            return;
        //Debug.Log("Connect clicked");

        // Check if server ip input field is empty
        if (gameManager.serverIpText.text.ToString().Trim() == "" ||
            gameManager.serverIpText.text.ToString().Trim() == string.Empty)
        {
            Debug.LogWarning("Server IP is empty");
            return;
        }
        // TODO: Validate server IP
        gameManager.StartClient();
    }
    private void OnDisconnectClicked()
    {
        // Si no está jugando, que no haga nada
        if (!isConnected)
            return;

        //Debug.Log("Disconnect clicked");

        gameManager.Disconnect();
    }
    private void HideUICanvas()
    {
        mainMenuCanvas.gameObject.SetActive(false);
    }

    private void ShowUICanvas()
    {
        mainMenuCanvas.gameObject.SetActive(true);
    }
    private void SwitchToGameCamera()
    {
        //Debug.Log("Switching to game camera");
        // Cambiar cámaras
        gameCamera.gameObject.SetActive(true);
        gameCamera.GetComponent<AudioListener>().enabled = true;

        menuCamera.GetComponent<AudioListener>().enabled = false;
        menuCamera.gameObject.SetActive(false);
        HideUICanvas();
    }

    private void SwitchToMenuCamera()
    {
        //Debug.Log("Switching to menu camera");
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
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerInput != null)
                playerInput.enabled = false;
            ChangeTeamMenu.Instance.HideUICanvas();
        }
        else
        {
            SwitchToGameCamera();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }

    private void Update()
    {


        // Si presiona la tecla A, el cursor se muestra
        if (Input.GetKeyDown(toggleKey) && isConnected)
        {
            //Debug.Log("Toggle pause menu");
            togglePauseMenu();
        }

        if (Input.GetMouseButton(0) && !pauseMenu && !ChangeTeamMenu.Instance.changeTeamMenuActive)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //Debug.Log("Application focused");
            gameFocused = true;
        }
        else
        {
            gameFocused = false;
        }

        if (pauseMenu && gameFocused)
        {
            //Debug.Log("Cursor visible");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            //Debug.Log("Cursor locked");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    public string GetPlayerName()
    {
        return playerNameInputField.text;
    }

    public void SetCanvasMessage(string message)
    {
        if (message != null || message != "")
        {
            messageTextBoxCanvas.gameObject.SetActive(true);
            messageTextBox.text = message;
        }
    }

    public void ResetCanvasMessage()
    {
        messageTextBox.text = "";
        messageTextBoxCanvas.gameObject.SetActive(false);
    }

    public void SetHealthText(int health)
    {
        healthTextBox.text = health.ToString();
    }
}

