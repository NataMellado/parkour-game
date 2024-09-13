using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

public class ClientNetworkAnimator : NetworkAnimator{

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}