using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Tbvl.GameManager
{
    public class GameManager : MonoBehaviour
    {
        
        public bool IsConnected;

        public void Start()
        {
            IsConnected = false;
        }

        public void StartHost(){
            NetworkManager.Singleton.StartHost();
            IsConnected = true;
        }

        public void StartClient(){
            NetworkManager.Singleton.StartClient();
            IsConnected = true;
        }
        
    }
}
