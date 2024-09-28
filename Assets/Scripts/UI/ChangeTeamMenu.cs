using System.Collections;
using System.Collections.Generic;
using Tbvl.GameManager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChangeTeamMenu : MonoBehaviour
{
    public static ChangeTeamMenu Instance { get; private set; }

    public bool changeTeamMenuActive = false;
    private bool canChangeTeam;
    public KeyCode toggleKey = KeyCode.M;

    [SerializeField]
    private Button selectCharacter_1;
    [SerializeField]
    private Button selectCharacter_2;
    [SerializeField]
    private Button selectCharacter_3;

    [SerializeField]
    private Button selectTeam1;
    [SerializeField]
    private Button selectTeam2;
    [SerializeField]
    private Button selectTeam3;
    
    [SerializeField]
    private Button closeTeamMenuButton;
    [SerializeField]
    private Canvas teamMenuCanvas;

    private Button currentSelectedButton = null;
    private Color normalColor = Color.white;
    private Color selectedColor = Color.green;
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

        HideUICanvas();

        selectCharacter_1.onClick.AddListener(() => HandleSelectCharacterButton(selectCharacter_1));
        selectCharacter_2.onClick.AddListener(() => HandleSelectCharacterButton(selectCharacter_2));
        selectCharacter_3.onClick.AddListener(() => HandleSelectCharacterButton(selectCharacter_3));

        selectTeam1.onClick.AddListener(() => HandleSelectTeamButton("selectTeam1"));
        selectTeam2.onClick.AddListener(() => HandleSelectTeamButton("selectTeam2"));
        selectTeam3.onClick.AddListener(() => HandleSelectTeamButton("selectTeam3"));

        closeTeamMenuButton.onClick.AddListener(toggleTeamMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (GameManager.Instance.IsConnected)
            {
                toggleTeamMenu();
            }
        }
        if (changeTeamMenuActive && Input.GetMouseButtonDown(1))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    /*
    private void SavePreviousButtonColor(Button button)
    {
        selectedButtonPreviousColor = button.colors;
    }

    private void UnmarkNormalColorButton(Button button)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = new Color(255, 255, 255, 255);
        button.colors = cb;
    }

    private void MarkSelectedButton(Button button)
    {
        if (previousSelectedButton == button)
            return;

        // Verificar si hay un bot�n ya seleccionado
        if (previousSelectedButton != null)
        {
            // Restaurar color del bot�n anterior
            UnmarkNormalColorButton(previousSelectedButton);
        }

        // Cambiar color del bot�n
        ColorBlock cb = button.colors;

        // Hacer el color un poco m�s oscuro
        // Asignar nuevo color
        cb.normalColor = new Color(175, 175, 175, 255);

        // Asignar nuevo color al bot�n
        button.colors = cb;

        // Asignar bot�n seleccionado
        previousSelectedButton = button;

    }*/

    void HandleSelectCharacterButton(Button selectedButton)
    {
        // Restaurar el color del bot�n previamente seleccionado
        if (currentSelectedButton != null)
        {
            SetButtonColor(currentSelectedButton, normalColor);
        }

        // Cambiar el color del bot�n actualmente seleccionado
        SetButtonColor(selectedButton, selectedColor);

        // Actualizar la referencia al bot�n seleccionado
        currentSelectedButton = selectedButton;

        // Aqu� puedes guardar la selecci�n del personaje seg�n el bot�n seleccionado
        // Por ejemplo, usar un identificador o �ndice
        // selectedCharacterId = ...

        Debug.Log("Bot�n seleccionado: " + selectedButton.name);
    }
    void SetButtonColor(Button button, Color color)
    {
        // Obtener el ColorBlock del bot�n
        ColorBlock cb = button.colors;

        // Actualizar los colores para cada estado del bot�n
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        cb.selectedColor = color;
        cb.disabledColor = color;

        // Asignar el ColorBlock modificado al bot�n
        button.colors = cb;
    }
    private void HandleSelectTeamButton(string buttonName)
    {
        switch (buttonName)
        {
            case "selectTeam1":
                break;
            case "selectTeam2":
                break;
            case "selectTeam3":
                break;
        }
    }

    public void HideUICanvas()
    {
        teamMenuCanvas.gameObject.SetActive(false);
        changeTeamMenuActive = false;
    }

    public void ShowUICanvas()
    {
        if (UIManager.Instance.pauseMenu)
            return;
        teamMenuCanvas.gameObject.SetActive(true);
        changeTeamMenuActive = true;
    }

    private void toggleTeamMenu()
    {
        if (changeTeamMenuActive)
        {
            HideUICanvas();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            ShowUICanvas();
        }
    }

}
