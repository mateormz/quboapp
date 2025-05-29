using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetShopController : MonoBehaviour
{
    public SkinsData skinsData;
    public Button[] botonesSkins;
    public TMP_Text textoMonedas;

    void Start()
    {
        textoMonedas.text = "Monedas: " + CurrencyManager.Instance.GetMonedas();
        CurrencyManager.Instance.OnMonedasActualizadas += ActualizarTextoMonedas;

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
            PlayerPrefs.SetInt("skinSeleccionada", index);
            return;
        }

        int precio = skinsData.preciosSkins[index];
        bool comprado = CurrencyManager.Instance.RestarMonedas(precio);

        if (comprado)
        {
            PlayerPrefs.SetInt("skin_" + index, 1);
            PlayerPrefs.SetInt("skinSeleccionada", index);
            VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
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
            botonesSkins[i].interactable = desbloqueada || monedasActuales >= skinsData.preciosSkins[i];
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTextoMonedas;
    }
}