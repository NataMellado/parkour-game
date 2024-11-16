using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPresentation : NetworkBehaviour
{
    private NetworkObject networkObject;
    public NetworkTransform networkTransform;
    public UIManager uiManager;
    public ThirdPersonController thirdPersonController;
    public StarterAssetsInputs starterAssetsInputs;


    [SerializeField]
    private List<GameObject> remoteOnly = new List<GameObject>();





    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }




    private bool prevIsOwner
    {
        get => internalPrevIsOwner;

        set
        {
            if (value != internalPrevIsOwner)
            {
                internalPrevIsOwner = value;
                OnChangedOwner(internalPrevIsOwner);
            }
        }
    }

    private void OnChangedOwner(bool v)
    {
        if (v)
        {
            //GetComponentInChildren<AudioListener>().enabled = true;
            GetComponent<ThirdPersonController>().enabled = true;
            GetComponent<CharacterController>().enabled = true;
            GetComponent<ClientNetworkAnimator>().enabled = true;
            GetComponent<ParkourController>().enabled = true;
        }
    }

    private bool internalPrevIsOwner;

    public override void OnNetworkSpawn()
    {
        Debug.Log("NetworkSpawn playerController");

        thirdPersonController = GetComponent<ThirdPersonController>();
        networkTransform = GetComponent<NetworkTransform>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        if (networkObject == null)
        {
            networkObject = GetComponent<NetworkObject>();
        }

    }



    private void Update()
    {
        prevIsOwner = IsOwner;
        
        //for (int i = 0; i < skinModels.Length; i++)
        //{
        //    Debug.Log(skinModels[i].name + " " + skinModels[i].activeSelf);
        //}
    }

    

    [ClientRpc]
    public void TeleportClientRpc(Vector3 newPosition, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {

            if (networkTransform != null)
            {
                thirdPersonController.enabled = false;
                thirdPersonController.isTeleporting = true;
                networkTransform.Teleport(newPosition, transform.rotation, transform.localScale);
                networkTransform.enabled = false;
            }
            else
            {
                transform.position = newPosition;
            }
        }
        StartCoroutine(ResetIsTeleporting());
    }

    [ClientRpc]
    public void FreezePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            //thirdPersonController.FreezePlayer();
            starterAssetsInputs.FreezePlayer();
        }
    }

    [ClientRpc]
    public void UnfreezePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            //thirdPersonController.UnfreezePlayer();
            starterAssetsInputs.UnfreezePlayer();
        }
    }

    [ClientRpc]
    public void LogToClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
            Debug.Log(message);
    }

    private IEnumerator ResetIsTeleporting()
    {
        yield return null; // Esperar un frame
        thirdPersonController.isTeleporting = false;
        thirdPersonController.enabled = true;
        networkTransform.enabled = true;
    }

    [ClientRpc]
    public void SetPlayerInGameMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
            uiManager.SetCanvasMessage(message);
    }

    [ClientRpc]
    public void ResetPlayerInGameMessageClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
            uiManager.ResetCanvasMessage();
    }
}
