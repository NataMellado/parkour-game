using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealthSync : NetworkBehaviour
{
    // NetworkVariable que sincroniza la vida del jugador
    public NetworkVariable<int> networkPlayerHealth =
        new NetworkVariable<int>(
            100,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);


    public GameObject bloodPrefab;

    public static int Damage = 25;

    public override void OnNetworkSpawn()
    {
        networkPlayerHealth.OnValueChanged += OnHealthChanged;

        if (IsOwner)
        {
            AssignHealthServerRpc();
            StartCoroutine(WaitAndInitialize());

        }
    }

    [ServerRpc]
    private void AssignHealthServerRpc(ServerRpcParams rpcParams = default)
    {
        try
        {
            networkPlayerHealth.Value = 100;
        }
        catch (System.Exception e)
        {
            Debug.Log("Error en AssignHealthServerRpc: " + e.Message);
        }
    }

    private IEnumerator WaitAndInitialize()
    {

        yield return null;

        ActualizarVida(networkPlayerHealth.Value);

    }

    private void OnDestroy()
    {
        networkPlayerHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {

            if (newValue <= 0)
            {
                NotifyDeathServerRpc();
                Debug.Log("Has muerto");
            }

            if (newValue < previousValue)
            {
                ActualizarVida(newValue);
                //BloodParticles();
            }
        }
    }

    private void ActualizarVida(int vida)
    {
        Debug.Log("Vida actual: " + vida);
        UIManager.Instance.SetHealthText(vida);
    }

    private void BloodParticles()
    {
        Vector3 position = transform.position;
        position.y += 1;
        GameObject explosion = Instantiate(bloodPrefab, position, Quaternion.identity);

        NetworkObject explosionNetworkObject = explosion.GetComponent<NetworkObject>();
        explosionNetworkObject.Spawn();
    }

    [ServerRpc]
    private void NotifyDeathServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Notificando muerte");
    }

}
