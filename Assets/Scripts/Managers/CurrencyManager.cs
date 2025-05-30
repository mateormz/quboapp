using UnityEngine;
using UnityEngine.UI;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int monedas = 0;

    public event Action<int> OnMonedasActualizadas;

    void Awake()
    {
        Instance = this;
        monedas = PlayerPrefs.GetInt("monedas", 0);
    }

    public void SumarMonedas(int cantidad)
    {
        monedas += cantidad;
        PlayerPrefs.SetInt("monedas", monedas);
        OnMonedasActualizadas?.Invoke(monedas);
    }

    public void RestarMonedas(int cantidad)
    {
        if (monedas < cantidad)
        {
            Debug.Log("No tienes suficientes monedas para realizar esta acción.");
            return;
        }

        monedas -= cantidad;
        PlayerPrefs.SetInt("monedas", monedas);
        OnMonedasActualizadas?.Invoke(monedas);
    }

    public int GetMonedas()
    {
        return monedas;
    }
}