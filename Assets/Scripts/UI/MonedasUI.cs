using UnityEngine;
using TMPro;

public class MonedasUI : MonoBehaviour
{
    public TMP_Text textoMonedas;

    void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            ActualizarTexto(CurrencyManager.Instance.GetMonedas());
            CurrencyManager.Instance.OnMonedasActualizadas += ActualizarTexto;
        }
        else
        {
            Debug.LogWarning("CurrencyManager no está inicializado aún.");
        }
    }

    void ActualizarTexto(int cantidad)
    {
        textoMonedas.text = "Monedas: " + cantidad;
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTexto;
        }
    }
}