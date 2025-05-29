using UnityEngine;

public class PetController : MonoBehaviour
{
    public SpriteRenderer petRenderer;
    public Sprite[] skinsCompletas; // Cada sprite es una skin diferente

    void Start()
    {
        Debug.Log(skinsCompletas.Length);
        int skinIndex = PlayerPrefs.GetInt("skinSeleccionada");
        petRenderer.sprite = skinsCompletas[skinIndex];
    }

    public void CambiarSkin(int index)
    {
        petRenderer.sprite = skinsCompletas[index];
        PlayerPrefs.SetInt("skinSeleccionada", index);
    }
}