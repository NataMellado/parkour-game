using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [SerializeField]
    public TMP_InputField skinIndex;

    private int SelectedCharacterIndex = 0;
    public void SelectSkin()
    {
        if (skinIndex.text.ToString().Trim() != "")
            try
            {
                SelectedCharacterIndex = int.Parse(skinIndex.text);
            }catch (System.Exception e)
            {
                Debug.LogError("Error al asignar Skin!: " + e.Message);
            }
    }
    public int GetSelectedCharacterIndex()
    {
        return SelectedCharacterIndex;
    }
}
