using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class SkinData
{
    public string skinName;
    public Avatar skinModel;
    public RuntimeAnimatorController animatorController;
    public GameObject skinObject;
}
public class PlayerSkinSync : NetworkBehaviour
{
    // NetworkVariable para sincronizar el nombre de la skin
    public NetworkVariable<FixedString128Bytes> networkSkinName =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes("Kachujin"),
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

    private PlayerTeamSync playerTeamSync;

    // Referencia al Animator del jugador
    public Animator animator;
    bool tieneAnimator;

    // Lista de skins disponibles
    [SerializeField]
    private SkinData[] skins;

    // Resto del código...
    public override void OnNetworkSpawn()
    {
        networkSkinName.OnValueChanged += OnSkinNameChanged;
        playerTeamSync = GetComponent<PlayerTeamSync>();
        playerTeamSync.networkPlayerTeam.OnValueChanged += OnTeamChanged;

        tieneAnimator = TryGetComponent(out animator);

        if (playerTeamSync == null)
        {
            Debug.LogError("PlayerTeamSync no encontrado en el jugador");
            return;
        }

        //if (IsOwner)
        //{
        //    //if (playerTeamSync.networkPlayerTeam.Value != PlayerTeamSync.Team.SinEquipo)
        //    //{
        //    //    AssignSkinServerRpc(playerTeamSync.networkPlayerTeam.Value);
        //    //}
        //    //else
        //    //{
        //    //    AssignSkinServerRpc();
        //    //}
        //    AssignSkinServerRpc();
        //    //StartCoroutine(CheckMySkin());
        //    StartCoroutine(WaitAndInitializeUI());

        //}

        //if (playerTeamSync.networkPlayerTeam.Value != PlayerTeamSync.Team.SinEquipo)
        //{
        //    OnTeamChanged(PlayerTeamSync.Team.SinEquipo, playerTeamSync.networkPlayerTeam.Value);
        //}
        //Debug.Log("Mi skin es: " + networkSkinName.Value.Value);
        // Actualizar la skin inicial
        //ActualizarSkin(networkSkinName.Value.Value);

        /*
        // Si somos el dueño, inicializamos nuestra skin
        if (IsOwner && tieneAnimator)
        {
            if (playerTeamSync == null)
            {
                Debug.LogError("PlayerTeamSync no encontrado en el jugador");
            }
            Debug.Log("Inicializando skin para un " + playerTeamSync.networkPlayerTeam.Value);
            // Obtener el nombre de la skin seleccionada (puede ser desde la UI o una predeterminada)
            string selectedSkinName = ObtenerNombreSkin();
            if (networkSkinName.Value.Value != selectedSkinName)
            {
                CambiarSkinServerRpc(selectedSkinName);
            }
        }
        // Actualizar la skin inicial
        */
        ActualizarSkin(networkSkinName.Value.Value);
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento
        networkSkinName.OnValueChanged -= OnSkinNameChanged;
        if (playerTeamSync != null)
        {
            playerTeamSync.networkPlayerTeam.OnValueChanged -= OnTeamChanged;
        }
    }

    private IEnumerator WaitAndInitializeUI()
    {
        // Esperar un frame para asegurar que las NetworkVariables estén sincronizadas
        yield return null;
        //Debug.Log("WaitAndInitializeUI: " + networkSkinName.Value.Value);
        if (networkSkinName.Value != "Kachujin")
        {
            //Debug.Log("Boom se actualiza: " + networkSkinName.Value.Value);
            ActualizarSkin(networkSkinName.Value.ToString());
        }
    }
    private IEnumerator CheckMySkin()
    {
        while (true)
        {
            try
            {

                //Debug.Log("CheckMySkin: " + OwnerClientId + " -> " + networkSkinName.Value);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error en CheckMySkin(): " + e.Message);
                yield break;
            }
            yield return new WaitForSeconds(2);
        };
    }



    public void CambiarSkin(string nuevaSkin)
    {
        if (IsOwner)
        {
            CambiarSkinServerRpc(nuevaSkin);
        }
    }

    private void OnTeamChanged(PlayerTeamSync.Team previousValue, PlayerTeamSync.Team newValue)
    {
        try
        {

            //Debug.Log($"[PlayerSkinSync] Equipo cambiado: {previousValue}, {newValue}");
            if (IsOwner)
            {
                AssignSkinServerRpc();
            }
            //ActualizarSkin()???
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en OnTeamChanged(): " + e.Message);
        }
    }

    private void OnSkinNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        try
        {
            ActualizarSkin(newValue.ToString());
            //Debug.Log("Skin cambiada a: " + newValue);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en OnSkinNameChanged(): " + e.Message);
        }
    }
    private void ActualizarSkin(string skinName)
    {
        bool skinFound = false;

        foreach (var skin in skins)
        {
            if (skin.skinModel != null)
            {
                // Activar o desactivar el modelo según corresponda
                bool isActive = skin.skinName == skinName;
                //Debug.Log("Revisando isActive: " + isActive + " para skin: " + skin.skinName + " comparando con : " + skinName);
                if (isActive)
                {
                    animator.avatar = skin.skinModel;
                    //Debug.Log("Colocando animator avatar: " + skin.skinName + "(" + skin.skinModel + ")");
                    skinFound = true;

                    //Debug.Log("Habilitando skinObject");
                    skin.skinObject.SetActive(isActive);

                    // Asignar el Animator correspondiente
                    if (animator != null && skin.animatorController != null)
                    {
                        animator.runtimeAnimatorController = skin.animatorController;
                    }
                    else
                    {
                        Debug.LogWarning("Animator o AnimatorController no asignado para la skin: " + skin.skinName);
                    }
                }
                else
                {
                    skin.skinObject.SetActive(isActive);
                }
            }
            else
            {
                Debug.LogWarning("skinModel no asignado para la skin: " + skin.skinName);
            }
        }

        if (!skinFound)
        {
            Debug.LogWarning("Skin no encontrada: " + skinName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignSkinServerRpc(ServerRpcParams rpcParams = default)
    {
        try
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            //FixedString128Bytes assignedSkin = GetAssignedSkin(assignedTeam);
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    var playerSkinSync = playerObject.GetComponent<PlayerSkinSync>();
                    if (playerSkinSync != null)
                    {
                        //Debug.LogWarning("Skin previa: " + playerSkinSync.networkSkinName.Value);
                        FixedString128Bytes assignedSkin = GetAssignedSkin(playerTeamSync.networkPlayerTeam.Value);
                        playerSkinSync.networkSkinName.Value = assignedSkin;
                        //Debug.Log($"[SERVER] Skin asignado a ID[{clientId}]: {playerSkinSync}");
                    }
                    else
                    {
                        Debug.LogError($"[SERVER] PlayerSkinSync no encontrado en el objeto del jugador para clientId: {clientId}");
                    }
                }
                else
                {
                    Debug.LogError($"[SERVER] PlayerSkinSync es null para clientId: {clientId}");
                }
            }
            else
            {
                Debug.LogError($"[SERVER] ConnectedClients no contiene clientId: {clientId}");
            }
            //Debug.Log("[SERVER] Skin asignado" + " a ID[" + clientId + "]: " + networkSkinName.Value);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SERVER] Error en AssignSkinServerRpc(): {e.Message}");
        }
    }
    private FixedString128Bytes GetAssignedSkin(PlayerTeamSync.Team equipo)
    {
        //Debug.Log("Obteniendo skin desde equipo: " + equipo.ToString());
        //Debug.Log("PlayerTeamSync: " + playerTeamSync.networkPlayerTeam.Value);

        switch (equipo)
        {
            case PlayerTeamSync.Team.Policias:
                return new FixedString128Bytes("Armature");
            case PlayerTeamSync.Team.Ladrones:
                return new FixedString128Bytes("Ch04");
            default:
                Debug.LogError("Equipo no asignado al jugador");
                return new FixedString128Bytes("Kachujin");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CambiarSkinServerRpc(string nuevaSkin)
    {
        try
        {

            //Debug.Log("Cambiando skin a: " + nuevaSkin);
            // Validar que la skin existe en la lista de skins
            bool skinValida = false;
            foreach (var skin in skins)
            {
                if (skin.skinName == nuevaSkin)
                {
                    skinValida = true;
                    break;
                }
            }

            if (skinValida && networkSkinName != null)
            {
                networkSkinName.Value = nuevaSkin;
            }
            else
            {
                Debug.LogWarning("Skin inválida solicitada por el cliente: " + OwnerClientId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en CambiarSkinServerRpc(): " + e.Message);
        }
    }
}





/*
 
     private Animator animator;
    private bool hasAnimator;
    public NetworkVariable<FixedString128Bytes> networkSkin =
    new NetworkVariable<FixedString128Bytes>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner);
    // Array de meshes o prefabs de skins
    public GameObject[] skinModels;

    // Array de animators de skins
    public Avatar[] skinAnimators;
    private void Start()
    {
        if (IsOwner)
        {
            hasAnimator = TryGetComponent(out animator);
            Debug.Log("Asignando skin de jugador");
            string skin = ObtenerSkinJugador();
            if (networkSkin.Value.Value != skin)
                networkSkin.Value = skin;
            UpdateSkin(networkSkin.Value.Value);
            networkSkin.OnValueChanged += OnSkinChanged;
        }
    }
    private void OnSkinChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        Debug.LogWarning("Skin cambiada");
        string skinName = newValue.Value ?? string.Empty;
        UpdateSkin(skinName);
    }

    private string ObtenerSkinJugador()
    {

        return "Kachujin";
    }
        StartCoroutine(ChangeSkinEach2Seconds());
    private IEnumerator ChangeSkinEach2Seconds()
    {
        while (true)
        {
            if (IsOwner)
            {
                int index = UnityEngine.Random.Range(0, skinModels.Length);
                networkSkin.Value = skinModels[index].name;
            }
            yield return new WaitForSeconds(4);
        }
    }
private void UpdateSkin(string skinName)
    {
        Debug.Log("Skin actual:" + networkSkin.Value);

        // verificar primero si skinName se contiene en el array de skins
        int index = -1;
        bool existe_skin = false;
        for (int i = 0; i < skinModels.Length; i++)
        {
            if (skinModels[i].name == skinName)
            {
                existe_skin = true;
                index = i;
                break;
            }
        }

        if (!existe_skin)
        {
            Debug.LogWarning("Skin no encontrada: " + skinName);
            return;
        }

        for (int i = 0; i < skinModels.Length; i++)
        {
            //skinModels[i].SetActive(i == index);
            Debug.Log("Skin: " + skinModels[i].name + " " + skinModels[i].activeSelf);
            if (skinModels[i].activeSelf)
            {
                skinModels[i].SetActive(false);
            }
            if (skinModels[i].name == skinName)
            {
                skinModels[i].SetActive(true);
            }
        }
        // activar animator de skin seleccionada
        for (int i = 0; i < skinAnimators.Length; i++)
        {

            Debug.Log("Nombre animator:" + skinAnimators[i].name);
            Debug.Log("runtime animator:" + animator.runtimeAnimatorController);
            if (skinAnimators[i].name == skinName)
            {
                animator.avatar = skinAnimators[i];
            }
        }

        Debug.Log(skinName);
    }

 
 */