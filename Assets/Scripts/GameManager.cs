using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Tbvl.GameManager
{
    public class GameManager : MonoBehaviour
    {
        public bool IsConnected
        {
            get => isConnected;
            private set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnConnectionStateChanged?.Invoke(isConnected); // Cuando cambia el estado de la conexión, se invoca el evento
                }
            }
        }

        private bool isConnected = false;

        public event System.Action<bool> OnConnectionStateChanged;

        public void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            IsConnected = false;
        }

        private void OnDestroy()
        {
            // Desuscribirse de los eventos cuando este objeto sea destruido
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                IsConnected = true;
        }
        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                IsConnected = false;
        }

        public void StartHost(){
            NetworkManager.Singleton.StartHost();
            StartCoroutine(WaitForConnection());
        }

        public void StartClient(){
            NetworkManager.Singleton.StartClient();
            StartCoroutine(WaitForConnection());
        }
        private IEnumerator WaitForConnection()
        {
            // Espera hasta que el cliente esté conectado
            yield return new WaitUntil(() => IsConnected);

            Debug.Log("Conectado al servidor");
            IsConnected = true;
        }
    }
}
