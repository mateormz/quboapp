using System;
using UnityEngine;

public class PetController : MonoBehaviour
{
    public SpriteRenderer petRenderer;
    public SkinsData skinsData;

    void Start()
    {
        int skinIndex = PlayerPrefs.GetInt("skinSeleccionada", 0);
        petRenderer.sprite = skinsData.skinsCompletas[skinIndex];

        Debug.Log("token: " + PlayerPrefs.GetString("token"));
        Debug.Log("user_id: " + PlayerPrefs.GetString("user_id"));
        Debug.Log("role: " + PlayerPrefs.GetString("role"));
    }
}