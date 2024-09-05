using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class PlayerPresentation : NetworkBehaviour
{
    private NetworkObject networkObject;

    [SerializeField]
    private List<GameObject> remoteOnly = new List<GameObject>();

    private bool prevIsOwner{
        get => internalPrevIsOwner;

        set {
            if (value != internalPrevIsOwner)
            {
                internalPrevIsOwner = value;
                OnChangedOwner(internalPrevIsOwner);
            }
        }
    }

    private void OnChangedOwner(bool v){
        if (v){
            GetComponentInChildren<AudioListener>().enabled = true;
            GetComponent<ThirdPersonController>().enabled = true;
            GetComponent<CharacterController>().enabled = true;
            GetComponent<ClientNetworkTransform>().enabled = true;
            foreach (GameObject g in remoteOnly){
                g.SetActive(false);
            }
        }
    }

    private bool internalPrevIsOwner;

    public override void OnNetworkSpawn()
    {
        Debug.Log("NetworkSpawn");

        if(networkObject == null){
            networkObject = GetComponent<NetworkObject>();
        }
    }

    private void Update() {
        prevIsOwner = IsOwner;
    }
}
