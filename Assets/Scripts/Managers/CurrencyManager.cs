using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

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

    public void SetMonedas(int cantidad)
    {
        monedas = cantidad;
        PlayerPrefs.SetInt("monedas", monedas);
        OnMonedasActualizadas?.Invoke(monedas);
    }

    public void SumarMonedas(int cantidad)
    {
        monedas += cantidad;
        PlayerPrefs.SetInt("monedas", monedas);
        OnMonedasActualizadas?.Invoke(monedas);
        StartCoroutine(ActualizarMonedasEnBackend("add", cantidad));
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
        StartCoroutine(ActualizarMonedasEnBackend("subtract", cantidad));
    }

    public int GetMonedas()
    {
        return monedas;
    }

    IEnumerator ActualizarMonedasEnBackend(string operation, int amount)
    {
        string user_id = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");
        string url = ApiConfig.UPDATE_USER_COINS(user_id);

        CoinOperation data = new CoinOperation { operation = operation, amount = amount };
        string jsonData = JsonUtility.ToJson(data);

        // Logs de debug
        Debug.Log("🧾 Enviando datos de monedas al backend...");
        Debug.Log($"🔑 Token: {token}");
        Debug.Log($"🧠 User ID: {user_id}");
        Debug.Log($"🌐 URL: {url}");
        Debug.Log($"📦 JSON: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();  
            request.SetRequestHeader("Authorization", token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log($"📥 Código de respuesta: {request.responseCode}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Monedas sincronizadas con backend");
                Debug.Log("🔄 Respuesta: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ Error al actualizar monedas en backend");
                Debug.LogError("🔄 Respuesta: " + request.downloadHandler.text);
                Debug.LogError("📥 Código HTTP: " + request.responseCode);
            }
        }
    }


    [Serializable]
    public class CoinOperation
    {
        public string operation;
        public int amount;
    }
}