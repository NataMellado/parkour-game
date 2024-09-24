using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

public class ClientNetworkTransform : NetworkTransform{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}