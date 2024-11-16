using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AutoDestroy : NetworkBehaviour
{
    public float lifetime = 5f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroyObject), lifetime);
        }
    }

    private void DestroyObject()
    {
        NetworkObject.Despawn(true);
    }
}
