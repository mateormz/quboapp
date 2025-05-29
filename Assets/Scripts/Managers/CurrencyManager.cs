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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Se mantiene entre escenas
            //monedas = PlayerPrefs.GetInt("monedas", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SumarMonedas(int cantidad)
    {
        monedas += cantidad;
        PlayerPrefs.SetInt("monedas", monedas);
        OnMonedasActualizadas?.Invoke(monedas);
    }

    public bool RestarMonedas(int cantidad)
    {
        if (monedas >= cantidad)
        {
            monedas -= cantidad;
            PlayerPrefs.SetInt("monedas", monedas);
            OnMonedasActualizadas?.Invoke(monedas);
            return true;
        }
        return false;
    }

    public int GetMonedas()
    {
        return monedas;
    }
}