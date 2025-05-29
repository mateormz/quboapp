using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetShopController : MonoBehaviour
{
    public Sprite[] skinsCompletas;           // Skins disponibles
    public int[] preciosSkins;                // Precios por skin
    public Button[] botonesSkins;             // Botones de cada skin
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
            int capturedIndex = i;
            botonesSkins[i].onClick.AddListener(() => ComprarSkin(capturedIndex));
        }

        VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
    }

    public void ComprarSkin(int index)
    {
        bool desbloqueada = PlayerPrefs.GetInt("skin_" + index, index == 0 ? 1 : 0) == 1;

        if (desbloqueada)
        {
            // Si ya está comprada, solo la selecciona
            PlayerPrefs.SetInt("skinSeleccionada", index);
            Debug.Log("Skin seleccionada: " + index);
            return;
        }

        int precio = preciosSkins[index];
        bool comprado = CurrencyManager.Instance.RestarMonedas(precio);

        if (comprado)
        {
            PlayerPrefs.SetInt("skin_" + index, 1);             // Desbloquear skin
            PlayerPrefs.SetInt("skinSeleccionada", index);     // Seleccionarla
            VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
            Debug.Log("Skin comprada y seleccionada: " + index);
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para esta skin.");
        }
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
            botonesSkins[i].interactable = desbloqueada || monedasActuales >= preciosSkins[i];
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTextoMonedas;
    }
}