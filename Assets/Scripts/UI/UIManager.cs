using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Tbvl.GameManager;

public class UIManager : MonoBehaviour
{
    
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    [SerializeField]
    private Canvas mainMenuCanvas;
    
    private GameManager gameManager;
    
    private void Awake() {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        gameManager = FindObjectOfType<GameManager>();
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

    private void Update() {
        // Si presiona la tecla A, el cursor se muestra
        if (Input.GetKeyDown(KeyCode.M)){
            Cursor.visible = true;
        }
    }
    
}

