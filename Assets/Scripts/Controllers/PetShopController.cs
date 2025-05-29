using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetShopController : MonoBehaviour
{
    public SpriteRenderer petRenderer;
    public Sprite[] skinsCompletas;           // Skins de la mascota
    public int[] preciosSkins;                // Precios de cada skin
    public Button[] botonesSkins;             // Botones para cada skin
    public TMP_Text textoMonedas;             // Texto con TMP para mostrar monedas

    void Start()
    {
        // Mostrar monedas actuales
        textoMonedas.text = "Monedas: " + CurrencyManager.Instance.GetMonedas();
        CurrencyManager.Instance.OnMonedasActualizadas += ActualizarTextoMonedas;

        // Configurar botones
        for (int i = 0; i < botonesSkins.Length; i++)
        {
            botonesSkins[i].interactable = true;

            // Capturar el índice en el listener
            int capturedIndex = i;
            botonesSkins[i].onClick.AddListener(() => ComprarSkin(capturedIndex));
        }

        // Verificar estado inicial de los botones
        VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
    }

    public void ComprarSkin(int index)
    {
        // Verifica si ya está desbloqueada
        if (PlayerPrefs.GetInt("skin_" + index, index == 0 ? 1 : 0) == 1)
        {
            CambiarSkin(index);
            return;
        }

        int precio = preciosSkins[index];
        bool comprado = CurrencyManager.Instance.RestarMonedas(precio);

        if (comprado)
        {
            PlayerPrefs.SetInt("skin_" + index, 1);
            CambiarSkin(index);
            VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para esta skin.");
        }
    }

    public void CambiarSkin(int index)
    {
        petRenderer.sprite = skinsCompletas[index];
        PlayerPrefs.SetInt("skinSeleccionada", index);
    }

    void ActualizarTextoMonedas(int nuevaCantidad)
    {
        textoMonedas.text = "Monedas: " + nuevaCantidad;
        VerificarBotonesPorMonedas(nuevaCantidad);
    }

    void VerificarBotonesPorMonedas(int monedasActuales)
    {
        for (int i = 0; i < botonesSkins.Length; i++)
        {
            bool desbloqueada = PlayerPrefs.GetInt("skin_" + i, i == 0 ? 1 : 0) == 1;

            if (desbloqueada)
            {
                botonesSkins[i].interactable = true;
            }
            else
            {
                botonesSkins[i].interactable = monedasActuales >= preciosSkins[i];
            }
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTextoMonedas;
        }
    }
}