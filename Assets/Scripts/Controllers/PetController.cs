using UnityEngine;

public class PetController : MonoBehaviour
{
    public SpriteRenderer petRenderer;
    public SkinsData skinsData;

    void Start()
    {
        int skinIndex = PlayerPrefs.GetInt("skinSeleccionada", 0);
        petRenderer.sprite = skinsData.skinsCompletas[skinIndex];
    }
}