using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CommandInterface : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField commandInputField;
    [SerializeField]
    private Button sendCommandButton;
    private string storedCommand = "";
    ulong clientId;
    private void Start()
    {
        if (CommandManager.Instance == null)
        {

            clientId = NetworkManager.Singleton.LocalClientId;
            Debug.LogError("No se encontró el CommandManager en la escena");
        }
        // Si el InputField pierde el foco, guarda el valor actual
        commandInputField.onDeselect.AddListener(OnCommandFieldDeselected);
        sendCommandButton.onClick.AddListener(OnSubmitCommand);
    }

    public void OnSubmitCommand()
    {
        string command = commandInputField.text;
        CommandManager.Instance.ExecuteCommand(clientId, command);
    }

    private void OnCommandFieldDeselected(string text)
    {
        storedCommand = commandInputField.text;
    }
}
